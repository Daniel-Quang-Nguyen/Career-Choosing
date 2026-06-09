using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Google.GenAI;
using Google.GenAI.Types;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. Data Model for Questions (Anti-Bias & De-gendered Version)
var questions = new List<Question>
{
    // Phase 1
    new("Your morning alarm goes off. What does getting ready look like?", "Following a structured, predictable routine", "Adjusting my pace based on how I feel that morning", "environmentPreference"),
    new("You are picking your outfit for school. What dictates your choice?", "Pure comfort, utility, and weather", "Aesthetics, expression, and the overall 'vibe'", "sourceOfTruth"),
    new("You miss the bus or your ride is late. How do you react?", "Take a moment to process the change before figuring it out", "Immediately shift focus to finding an alternative option", "stressResilience"),
    new("You walk into a loud, crowded high school hallway.", "Keep to myself and head straight to my destination", "Look around to spot friends and say hi", "socialOrientation"),
    new("You look at your locker or backpack organization.", "Everything is in exact, labeled folders", "Items are loosely placed, but I know where everything is", "environmentPreference"),
    new("You read the morning announcements on the board.", "I focus on gathering the main headline", "I naturally notice small formatting or spelling details", "detailFocus"),
    new("You are listening to a podcast or music on the way to school. What catches your brain?", "Concrete, real-world true stories or news", "Deep, metaphorical concepts or complex lyrical storytelling", "abstractThinking"),

    // Phase 2
    new("1st Period gives you a major project with NO rubric.", "I prefer having clear guidelines and criteria to follow", "I welcome the flexibility to design my own approach", "ambiguityTolerance"),
    new("You have to learn a complex new software for a class.", "Read the documentation or watch a comprehensive tutorial first", "Explore the interface hands-on to see how it reacts", "cognitivePace"),
    new("Which class discussion makes your brain light up?", "Calculating physical, concrete measurements", "Debating an abstract philosophical question", "abstractThinking"),
    new("In History, you analyze an event from a perspective you disagree with.", "I prefer focusing on established, objective facts", "I find value in exploring contrasting viewpoints", "ambiguityTolerance"),
    new("In Science lab, your experiment fails the textbook result.", "Focus on correcting the procedure to achieve the expected result", "Investigate the unexpected variables to see why it diverged", "riskAppetite"),
    new("You are solving a complex math equation.", "Follow the clean, orderly steps exactly", "Look for an unorthodox or creative shortcut", "cognitivePace"),
    new("You are reviewing peer-reviewed study sheets.", "I trust the high-level summary concept entirely", "I cross-reference the data points and indices to ensure no metrics are missed", "detailFocus"),

    // Phase 3
    new("It's lunchtime. Where do you recharge?", "A quiet space alone or with one close friend", "A lively, energetic area with a larger group", "socialOrientation"),
    new("A friend is sharing a major personal problem with you.", "Analyze the root cause and help map out a practical fix", "Focus entirely on comforting, listening, and validating them", "sourceOfTruth"),
    new("You and your friends plan a weekend hangout.", "A structured itinerary of where to go and when", "A flexible plan where we figure things out as we go", "environmentPreference"),
    new("Your friend group suddenly changes plans 5 minutes before meeting.", "I prefer sticking to the originally agreed schedule", "I am completely comfortable adapting to the new plan", "stressResilience"),
    new("A system or project you are working on breaks.", "Review the logs and troubleshoot it independently in silence", "Start a group conversation to brainstorm solutions out loud", "socialOrientation"),
    
    // ATTENTION CHECK 1: Chống lỗi bất cẩn bấm đại (Carelessness Bias)
    new("To ensure you are reading carefully, please drag the slider completely to the LEFT.", "I am reading carefully", "I am skipping questions", "attentionCheck_Left"),

    new("You hear a rumor or half-story about some school drama.", "I prefer knowing the exact, factual truth right away", "I am comfortable letting it go without needing the full context", "ambiguityTolerance"),
    new("Two people are having an ideological debate. How do you judge who is right?", "By evaluating cold, hard empirical data and historical references", "By assessing who has the more compassionate and morally sound approach", "sourceOfTruth"),

    // Phase 4
    new("If you worked on a creative project, what is your role?", "Production Manager (Budgets, schedules, and logistics)", "Writer/Director (Creative vision, overarching plot)", "outputFocus"),
    
    // GENDER BIAS FIX: Bối cảnh phi giới tính hóa
    new("You have free time in a creative workshop or design studio.", "Optimizing and repairing an old, broken system until it runs smoothly", "Drafting a completely new, experimental concept from scratch", "outputFocus"),
    
    new("You are building a complex LEGO set.", "Follow the instruction manual page-by-page", "Look at the box photo and assemble a custom version", "environmentPreference"),
    new("Your group project partner cannot contribute the night before it's due.", "Carefully adjust the remaining workload to handle the fallout step-by-step", "Channel the urgency into highly focused, direct execution", "stressResilience"),
    new("You are picked to lead a student committee.", "Backstage coordinator (Managing logistics, operations, and rules)", "Front-facing leader (Delivering speeches, setting vision, and motivation)", "socialOrientation"),
    new("You are writing a major essay.", "Creating a detailed, bulleted outline before drafting", "Writing continuously to capture ideas, then organizing them later", "cognitivePace"),
    new("You have built a functional prototype app or project. What's your next move?", "Spend extra time polishing the interface and aligning minor details", "Launch it immediately to gather feedback on the core concept", "outputFocus"),

    // Phase 5
    new("You are playing a strategic video/board game.", "A measured, defensive strategy that minimizes mistakes", "A bold, dynamic strategy with high risks but immediate rewards", "riskAppetite"),
    new("Your teammate makes a significant mistake, altering the game dynamic.", "Feel standard disappointment that the established order was disrupted", "See it as an interesting new tactical puzzle to solve", "stressResilience"),
    new("You are shopping for a new laptop or phone.", "Comparing numerical spec sheets, performance, and benchmarks", "Evaluating the overall industrial design, color, and user experience", "sourceOfTruth"),
    new("How do you manage your computer's desktop files?", "Strictly organized into clearly labeled folders", "Keep files easily accessible on the desktop and rely on the search bar", "environmentPreference"),
    new("You are reading a mystery book or watching a movie.", "I enjoy following the story organically at the author's pace", "I actively try to analyze the clues and deduce the ending ahead of time", "cognitivePace"),
    new("A teacher assigns a concept your class hasn't covered yet.", "Wait for formal instruction or guidelines before attempting it", "Spend time exploring resources independently to decipher it", "riskAppetite"),

    // ATTENTION CHECK 2: Gài bẫy ở pha mệt mỏi
    new("To verify your focus, please drag the slider completely to the RIGHT.", "I am clicking blindly", "I am fully engaged", "attentionCheck_Right"),

    // Phase 6
    new("When you look back at a major historical crisis, what catches your focus?", "The exact timeline, dates, and specific decisions made by leaders", "The big-picture causes, human psychology, and hidden patterns", "abstractThinking"),
    new("You are finalizing a video edit or creative digital art piece.", "Once the core message is clear, I consider it complete", "I will invest significant time refining individual frames or pixel alignments", "detailFocus"),
    new("You are researching a topic online. What is your pattern?", "Open one tab, reading it comprehensively from top to bottom before moving on", "Open multiple tabs simultaneously, rapidly scanning and jumping between insights", "cognitivePace"),
    new("You have to give an important class presentation.", "Write out a detailed script to ensure every point is covered safely", "Use basic bullet points and deliver the explanation naturally on stage", "riskAppetite"),
    new("Your home internet goes offline 1 hour before a massive project submission.", "Take a brief moment to stabilize the pressure before finding a backup plan", "Immediately switch to tactical troubleshooting (using hotspots or emailing alternatives)", "stressResilience"),
    new("You are playing a new open-world video game with no quest markers.", "I prefer having a clear map or indicators directing my progression", "I prefer exploring freely without directions to discover my own path", "ambiguityTolerance"),

    // Phase 7 — Humanities Metrics
    new("You are reading two articles: one is packed with statistics, the other is a personal narrative. Which do you trust more?", "The data and statistics — objective metrics provide clarity", "The human story — personal context and lived experiences provide meaning", "narrativeThinking"),
    new("A new law is being debated. How do you evaluate whether it's a good idea?", "By analyzing its measurable outcomes and economic data", "By asking whether it's fair and how it protects vulnerable communities", "ethicalOrientation"),
    new("You are assigned a history project. What angle excites you most?", "Future implications — what this event changed going forward", "Deep context — understanding the historical conditions that caused the event", "temporalFocus"),
    new("When you need to explain a complex idea, what feels most natural?", "Drawing a diagram, chart, or writing structured formulas", "Writing an essay, giving a speech, or crafting a metaphor", "expressionMode"),
    new("You are watching a documentary. Which topic keeps you glued to the screen?", "How a specific individual changed history through their unique vision", "How entire cultures, economies, or societies rise and evolve collectively", "humanSystemsInterest"),
    new("You are editing someone else's writing. What do you notice first?", "Whether the argument is logically structured and factually accurate", "Whether the tone, word choice, and voice feel authentic and compelling", "languageAffinity"),
    new("Two people in your friend group are in a serious conflict. What's your instinct?", "Help determine what objectively happened to identify the root cause", "Facilitate an open conversation to help them understand each other's perspectives", "conflictApproach"),
    new("Why do you learn new things?", "To build something, solve a problem, or create a tangible result", "To expand my understanding of the world, regardless of practical application", "knowledgePursuit"),
    new("A philosopher claims that truth is experienced differently by every culture. Your reaction?", "Truth should ideally be objective, verifiable, and universally applicable", "Reality is naturally filtered through human, cultural, and subjective frameworks", "structureOfTruth"),
};

// 2. Endpoint to Serve the HTML Frontend Interactively
app.MapGet("/", () => Results.Content(GetHtmlContent(questions), "text/html"));

// 3. Endpoint to Process Responses and Query Gemini
// Sử dụng Dictionary<string, int> để tránh lỗi Submission/Binding Error khi đọc file JSON
app.MapPost("/api/evaluate", async ([FromBody] Dictionary<string, int> rawAnswers) =>
{
    // Chuyển đổi an toàn từ chuỗi sang số nguyên
    var answers = new Dictionary<int, int>();
    foreach (var kvp in rawAnswers)
    {
        if (int.TryParse(kvp.Key, out int index))
        {
            answers[index] = kvp.Value;
        }
    }

    // --- BỘ LỌC CHỐNG BẤT CẨN (CARELESSNESS FILTER) ---
    for (int i = 0; i < questions.Count; i++)
    {
        if (questions[i].MetricKey == "attentionCheck_Left" && answers.TryGetValue(i, out int leftVal) && leftVal > 2)
        {
            return Results.Json(new { error = "Please read the questions carefully. It looks like you missed an attention check." }, statusCode: 400);
        }
        if (questions[i].MetricKey == "attentionCheck_Right" && answers.TryGetValue(i, out int rightVal) && rightVal < 8)
        {
            return Results.Json(new { error = "Please read the questions carefully. It looks like you missed an attention check." }, statusCode: 400);
        }
    }

    var activeMetrics = questions.Where(q => !q.MetricKey.StartsWith("attentionCheck")).Select(q => q.MetricKey).Distinct();
    var rawMetrics = activeMetrics.ToDictionary(k => k, k => 0);
    var metricMaxCounts = questions.Where(q => !q.MetricKey.StartsWith("attentionCheck")).GroupBy(q => q.MetricKey).ToDictionary(g => g.Key, g => g.Count());

    for (int i = 0; i < questions.Count; i++)
    {
        if (questions[i].MetricKey.StartsWith("attentionCheck")) continue;

        if (answers.TryGetValue(i, out int score))
        {
            rawMetrics[questions[i].MetricKey] += (score - 5);
        }
    }

    var normalizedMetrics = new Dictionary<string, string>();
    foreach (var metric in rawMetrics)
    {
        int maxPossibleScore = metricMaxCounts[metric.Key] * 4; 
        double percentage = maxPossibleScore == 0 ? 0 : Math.Round(((double)metric.Value / maxPossibleScore) * 100);
        string formattedPercent = percentage > 0 ? $"+{percentage}%" : $"{percentage}%";
        normalizedMetrics[metric.Key] = formattedPercent;
    }

    var apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY")
        ?? throw new InvalidOperationException("GEMINI_API_KEY is not set.");

    var client = new Client(apiKey: apiKey);
    var telemetryList = string.Join("\n", normalizedMetrics.Select(m => $"- {m.Key}: {m.Value}"));

    string systemPrompt = $$"""
        You are a friendly, encouraging career mentor. Your task is to analyze a high school student's test results and give them advice.
        
        The provided data is on a scale from -100% to +100%. 
        - A negative number means they prefer the Left Trait (e.g., structure, facts, rules, planning).
        - A positive number means they prefer the Right Trait (e.g., freedom, ideas, feelings, adapting).
        - Near 0% means they are balanced and can do both.

        Your tasks:
        1. Explain their "Big Five" (OCEAN) personality profile in simple terms based on their scores.
        2. Suggest exactly 3 good career paths for them.
        3. List 4 to 6 other jobs they might like.
        4. Write a short, happy concluding paragraph to cheer them on.

        LANGUAGE AND READABILITY RULES (CRITICAL):
        - Use CEFR B1-B2 level English ONLY.
        - Write as if you are talking to a 15-year-old high school student.
        - Keep sentences short and clear. Use simple, everyday words.
        - DO NOT use complex words (e.g., use "use" instead of "utilize", "help" instead of "ameliorate", "clear" instead of "lucid").
        - DO NOT use corporate or academic jargon (No words like "telemetry", "paradigm", "intersectionality", or "synergy").

        BIAS & DIVERSITY RULES:
        - Gender Neutrality: Do not base jobs on boy/girl stereotypes. Focus only on their scores.
        - Radical Diversity: The 3 core jobs must be very different from each other (e.g., one tech job, one creative job, one hands-on/trade job).
        - Grounded Reality: Pick normal, real jobs (e.g., Teacher, Plumber, Nurse, App Developer, City Planner) but not too generic (e.g., not just "Engineer" or "Artist" -> it should be more specific). Avoid futuristic jobs that might not exist in 10 years.

        Formatting & Style:
        - Use "You" and "Your".
        - Do not show them the math, percentages, or variable names. Just explain what the numbers mean in plain English. (e.g., "You have a strong preference for structure and clear rules" instead of "Your conscientiousness is +80%").
        - Do not change the five OCEAN trait names, but explain them in simple terms. -> correspond Openness, Conscientiousness, Extraversion, Agreeableness, Neuroticism to the traits you have. -> create a scale for each trait and base on that to improve the explanation.
        - The Ocean profile should be a small paragraph (about 3-4 sentences) for each trait, not just a sentence. Use their scores to explain what they are like and how it might affect their work style and preferences.



        - CRITICAL: DO NOT change the 5 OCEAN trait names in the final JSON output. They must be exactly "openness", "conscientiousness", "extraversion", "agreeableness", and "neuroticism". The career paths must have the exact fields: "title", "whyThisFits", "workOpportunities", "dayInTheLife", and "challengeAndGrowth". The final output MUST be a valid JSON object with these exact keys and structure.
        YOU MUST RETURN A VALID JSON OBJECT EXACTLY LIKE THIS:
        {
            "oceanProfile": {
                "openness": "Detailed analysis of their intellectual curiosity, creative imagination, and willingness to embrace unconventional ideas and new experiences.",
                "conscientiousness": "Detailed analysis of their reliability, organizational skills, attention to detail, and ability to stay focused on long-term goals.",
                "extraversion": "Detailed analysis of their sociability, assertiveness, enthusiasm, and whether they draw energy from interacting with others or from solitude.",
                "agreeableness": "Detailed analysis of their empathy, trustworthiness, cooperative nature, and how much they prioritize social harmony over personal conflict.",
                "neuroticism": "Detailed analysis of their emotional stability, resilience under pressure, and susceptibility to experiencing negative emotions like anxiety or mood swings."
            },
            "careerPaths": [
                {
                    "title": "Job Title",
                    "whyThisFits": "A precise psychological justification linking their specific OCEAN scores directly to the core demands of this role. Explain exactly how their cognitive style, social battery, and stress tolerance will give them a competitive advantage—or fulfill them deeply—in this line of work.",
                    "workOpportunities": "A data-driven market outlook for this role. Identify 2-3 specific high-growth sectors or niche industries where this role is critical. Include realistic entry-level vs. senior salary benchmarks, 5-year growth projections, and name-drop 2 specific, real-world companies or innovative products that utilize this exact role to illustrate market demand.",
                    "dayInTheLife": "A vivid, chronological breakdown of a typical Tuesday in this role. Detail the exact technical tools, software, or frameworks they will use, the cross-functional team members they will collaborate with (e.g., PMs, engineers, stakeholders), and a realistic look at the operational pace and work environment (remote, hybrid, or high-collaboration).",
                    "challengeAndGrowth": "An honest breakdown of the most common point of burnout or friction in this role. Identify a specific systemic challenge (e.g., creative blocks, scope creep, stakeholder conflict) and counter it with an advanced, constructive growth strategy or industry-standard coping framework to ensure long-term resilience and career longevity."
                }
            ],
            "otherFavorableJobs": [ "Job A", "Job B", "Job C", "Job D", "Job E" ],
            "overallSynthesis": "A final happy message to motivate them and other paths they could explore in the future. Emphasize that they have many options and that their unique combination of traits can lead to success in various fields. Encourage them to keep an open mind and continue exploring their interests as they grow."
        }

        Data to analyze:
        {{telemetryList}}
        """;

    try
    {
        var response = await client.Models.GenerateContentAsync(
            model: "gemini-3.1-flash-lite", 
            contents: "Analyze my profile. Use simple B1-B2 English, ensure diverse career options, and return the JSON.",
            config: new GenerateContentConfig
            {
                SystemInstruction = new Content { Parts = new List<Part> { new() { Text = systemPrompt } } },
                Temperature = 0.7f,
                ResponseMimeType = "application/json" 
            }
        );

        string reportText = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                            ?? throw new Exception("No report was generated by the AI.");

        return Results.Content(reportText, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = $"Server error: {ex.Message}" }, statusCode: 500);
    }
});

// 4. Start the app dynamically based on the environment
var port = System.Environment.GetEnvironmentVariable("PORT");

if (string.IsNullOrEmpty(port))
{
    app.Run(); 
}
else
{
    app.Run($"http://0.0.0.0:{port}"); 
}

// 5. Global View Engine Generation Function
string GetHtmlContent(List<Question> questionSet)
{
    var listHtml = string.Join("\n", questionSet.Select((q, index) => $$"""
        <div class="question-card flex flex-col border-2 border-gray-400 rounded-lg overflow-hidden mb-10 shadow-lg bg-white">
            <div class="bg-white text-black p-8 text-center border-b-2 border-gray-400">
                <h3 class="text-3xl">{{index + 1}}. {{q.Text}}</h3>
            </div>
            <div class="flex w-full border-b-2 border-gray-400">
                <div class="w-1/2 bg-white text-black p-10 text-center flex items-center justify-center">
                    <span class="text-xl font-medium">{{q.LowLabel}}</span>
                </div>
                <div class="w-1/2 bg-[#1a1a1a] text-white p-10 text-center flex items-center justify-center">
                    <span class="text-xl font-medium">{{q.HighLabel}}</span>
                </div>
            </div>
            <div class="flex w-full relative">
                <div class="absolute inset-0 flex">
                    <div class="w-1/2 bg-white"></div>
                    <div class="w-1/2 bg-[#1a1a1a]"></div>
                </div>
                <div class="relative z-10 flex w-full items-center py-6 px-8">
                    <span class="text-3xl font-bold text-black mr-6">←</span>
                    <input type="range" min="1" max="9" value="5" step="1" name="q_{{index}}" class="custom-slider w-full mx-2">
                    <span class="text-3xl font-bold text-white ml-6">→</span>
                </div>
            </div>
        </div>
    """));

    return $$"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Career Discovery Engine</title>
        <script src="https://cdn.tailwindcss.com"></script>
        <style>
            body {
                background-color: #121212;
                font-family: 'DejaVu Serif', Georgia, serif;
            }
            .custom-slider {
                -webkit-appearance: none;
                appearance: none;
                background: transparent;
                cursor: pointer;
            }
            .custom-slider::-webkit-slider-runnable-track {
                background: #666666;
                height: 8px;
                border-radius: 4px;
            }
            .custom-slider::-moz-range-track {
                background: #666666;
                height: 8px;
                border-radius: 4px;
            }
            .custom-slider::-webkit-slider-thumb {
                -webkit-appearance: none;
                appearance: none;
                margin-top: -12px;
                background-color: #222222;
                height: 32px;
                width: 32px;
                border-radius: 50%;
                border: 2px solid #ffffff;
                box-shadow: 0 2px 5px rgba(0,0,0,0.3);
            }
            .custom-slider::-moz-range-thumb {
                background-color: #222222;
                height: 32px;
                width: 32px;
                border-radius: 50%;
                border: 2px solid #ffffff;
                box-shadow: 0 2px 5px rgba(0,0,0,0.3);
            }
        </style>
    </head>
    <body class="text-gray-900 min-h-screen antialiased">
        <div class="max-w-4xl mx-auto px-4 py-12">
            <header class="text-center mb-12">
                <h1 class="text-4xl sm:text-6xl font-black tracking-tight text-white mb-6 font-sans">
                    YOUR CAREER PATH
                </h1>
                <p class="text-gray-400 text-xl font-medium font-sans">Move the sliders to show what fits you best</p>
            </header>

            <form id="quizForm" class="space-y-8">
                {{listHtml}}
                <div class="text-center pt-8">
                    <button type="submit" id="submitBtn" class="w-full sm:w-auto bg-black hover:bg-gray-800 text-white font-bold px-12 py-5 rounded-xl shadow-lg transform transition active:scale-95 text-xl font-sans">
                        See My Results
                    </button>
                </div>
            </form>

            <div id="outputContainer" class="hidden mt-12 space-y-8">
                <div id="loadingState" class="bg-white border border-gray-300 rounded-2xl p-10 shadow-2xl text-center">
                    <div class="animate-pulse flex flex-col items-center">
                        <div class="w-12 h-12 border-4 border-black border-t-transparent rounded-full animate-spin mb-4"></div>
                        <h2 class="text-2xl font-bold text-gray-800 font-sans">Looking for the perfect match...</h2>
                        <p class="text-gray-500 mt-2">Please wait a few seconds.</p>
                    </div>
                </div>

                <div id="resultsUI" class="hidden space-y-8">
                    <div class="bg-gray-50 border-l-8 border-black rounded-r-2xl p-8 shadow-lg">
                        <h2 class="text-3xl font-black text-black mb-6 font-sans uppercase">Your Personality Profile</h2>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 text-lg" id="oceanContent"></div>
                    </div>

                    <div id="careersContent" class="space-y-6"></div>
                    <div id="otherJobsContainer"></div>

                    <div class="bg-black text-white rounded-2xl p-8 shadow-2xl">
                        <h2 class="text-3xl font-black mb-4 font-sans uppercase">Final Thoughts</h2>
                        <p id="synthesisContent" class="text-xl leading-relaxed text-gray-200"></p>
                    </div>
                </div>
            </div>
        </div>

        <script>
            document.getElementById('quizForm').addEventListener('submit', async (e) => {
                e.preventDefault();
                const btn = document.getElementById('submitBtn');
                const outContainer = document.getElementById('outputContainer');
                const loadingState = document.getElementById('loadingState');
                const resultsUI = document.getElementById('resultsUI');
                
                btn.disabled = true;
                btn.classList.add('opacity-50', 'cursor-not-allowed');
                btn.innerText = "Thinking...";
                
                outContainer.classList.remove('hidden');
                loadingState.classList.remove('hidden');
                resultsUI.classList.add('hidden');
                outContainer.scrollIntoView({ behavior: 'smooth' });
                
                const payload = {};
                new FormData(e.target).forEach((value, key) => {
                    const index = key.replace('q_', '');
                    payload[index] = parseInt(value);
                });

                try {
                    const res = await fetch('/api/evaluate', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    });
                    
                    const data = await res.json();
                    
                    if(!res.ok || data.error) {
                        alert(data.error || "Something went wrong. Please try again.");
                        outContainer.classList.add('hidden');
                    } else {
                        renderResults(data);
                        loadingState.classList.add('hidden');
                        resultsUI.classList.remove('hidden');
                    }
                } catch (err) {
                    alert("Network error. Please check your connection.");
                    console.error(err);
                    outContainer.classList.add('hidden');
                } finally {
                    btn.disabled = false;
                    btn.classList.remove('opacity-50', 'cursor-not-allowed');
                    btn.innerText = "See My Results";
                }
            });

            function renderResults(data) {
                const oceanHtml = `
                    <div><span class="font-bold text-gray-900 block">Openness</span> <span class="text-gray-700">${data.oceanProfile.openness}</span></div>
                    <div><span class="font-bold text-gray-900 block">Conscientiousness</span> <span class="text-gray-700">${data.oceanProfile.conscientiousness}</span></div>
                    <div><span class="font-bold text-gray-900 block">Extraversion</span> <span class="text-gray-700">${data.oceanProfile.extraversion}</span></div>
                    <div><span class="font-bold text-gray-900 block">Agreeableness</span> <span class="text-gray-700">${data.oceanProfile.agreeableness}</span></div>
                    <div class="md:col-span-2"><span class="font-bold text-gray-900 block">Neuroticism</span> <span class="text-gray-700">${data.oceanProfile.neuroticism}</span></div>
                `;
                document.getElementById('oceanContent').innerHTML = oceanHtml;

                const careersHtml = data.careerPaths.map(career => `
                    <div class="bg-white border border-gray-300 rounded-2xl p-8 shadow-xl transition-all hover:shadow-2xl hover:-translate-y-1">
                        <h3 class="text-3xl font-bold text-black mb-6 font-sans border-b-2 border-gray-100 pb-4">${career.title}</h3>
                        <div class="space-y-4 text-lg text-gray-800">
                            <p><strong class="text-black block mb-1">Why This Fits You:</strong> ${career.whyThisFits}</p>
                            <p><strong class="text-black block mb-1">Work Opportunities:</strong> ${career.workOpportunities}</p>
                            <p><strong class="text-black block mb-1">A Day in Your Life:</strong> ${career.dayInTheLife}</p>
                            <div class="bg-red-50 text-red-900 p-4 rounded-xl mt-6 border border-red-100">
                                <strong class="block mb-1">A Challenge & How to Grow:</strong>
                                ${career.challengeAndGrowth}
                            </div>
                        </div>
                    </div>
                `).join('');
                document.getElementById('careersContent').innerHTML = careersHtml;

                if (data.otherFavorableJobs && data.otherFavorableJobs.length > 0) {
                    const otherJobsTags = data.otherFavorableJobs.map(job => 
                        `<span class="inline-block bg-gray-200 text-gray-900 px-4 py-2 rounded-full text-sm font-bold shadow-sm border border-gray-300">${job}</span>`
                    ).join('');
                    
                    document.getElementById('otherJobsContainer').innerHTML = `
                        <div class="bg-white border border-gray-300 rounded-2xl p-8 shadow-md">
                            <h3 class="text-2xl font-bold text-black mb-4 font-sans">Other Great Jobs for You</h3>
                            <div class="flex flex-wrap gap-3">
                                ${otherJobsTags}
                            </div>
                        </div>
                    `;
                }

                document.getElementById('synthesisContent').innerText = data.overallSynthesis;
            }
        </script>
    </body>
    </html>
    """;
}

public record Question(string Text, string LowLabel, string HighLabel, string MetricKey);