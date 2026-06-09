using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Google.GenAI;
using Google.GenAI.Types;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. Data Model for Questions
var questions = new List<Question>
{
    // Phase 1
    new("Your morning alarm goes off. What does getting ready look like?", "Strict minute-by-minute routine", "Wing it based on how I feel", "environmentPreference"),
    new("You are picking your outfit for school. What dictates your choice?", "Pure comfort, utility, and weather", "Aesthetics, expression, and the 'vibe'", "sourceOfTruth"),
    new("You miss the bus or your ride is late. How do you react?", "Deep internal panic", "Instantly adapt and find a new solution", "stressResilience"),
    new("You walk into a loud, crowded high school hallway.", "Put headphones in and avoid eye contact", "Look around to spot friends and say hi", "socialOrientation"),
    new("You look at your locker or backpack organization.", "Everything is in exact, labeled folders", "Absolute chaos, but I know where things are", "environmentPreference"),
    new("You read the morning announcements on the board.", "I just skim the main headline", "I immediately notice a spelling typo in the text", "detailFocus"),
    new("You are listening to a podcast or music on the way to school. What catches your brain?", "Concrete, real-world true stories or news", "Deep, metaphorical concepts or complex lyrical storytelling", "abstractThinking"),

    // Phase 2
    new("1st Period gives you a major project with NO rubric.", "Panic / Tell me exactly what to do", "Freedom / I love making my own rules", "ambiguityTolerance"),
    new("You have to learn a complex new software for a class.", "Read the manual / Watch a 30-min tutorial", "Click buttons randomly until it works", "cognitivePace"),
    new("Which class discussion makes your brain light up?", "Calculating physical, concrete measurements", "Debating an abstract philosophical question", "abstractThinking"),
    new("In History, you analyze an event from a perspective you disagree with.", "Frustrating / I prefer objective facts", "Effortless / I like exploring alternate views", "ambiguityTolerance"),
    new("In Science lab, your experiment fails the textbook result.", "Frustrated the lab is ruined", "Curious to debug the rogue variables", "riskAppetite"),
    new("You are solving a complex math equation.", "Follow the clean, orderly steps exactly", "Hunt for an unorthodox shortcut", "cognitivePace"),
    new("You are reviewing peer-reviewed study sheets.", "I trust the summary concept entirely", "I cross-reference the data points and indices to ensure no metrics are missed", "detailFocus"),

    // Phase 3
    new("It's lunchtime. Where do you recharge?", "Quiet library alone or with one person", "The loudest, most crowded table", "socialOrientation"),
    new("A friend is crying over a major personal problem.", "Analyze the root cause and map out a fix", "Focus entirely on comforting and validating them", "sourceOfTruth"),
    new("You and your friends plan a weekend hangout.", "Strict itinerary of where to go and when", "Just show up and figure it out", "environmentPreference"),
    new("Your friend group suddenly changes plans 5 minutes before meeting.", "I hate last-minute disruptions", "I don't mind, go with the flow", "stressResilience"),
    new("A system or project you are working on breaks.", "Pull up the logs and fix it solo in silence", "Call a group chat to brainstorm out loud", "socialOrientation"),
    new("You hear a rumor or half-story about some school drama.", "I need to know the exact factual truth immediately", "I'm comfortable not knowing the full context", "ambiguityTolerance"),
    new("Two friends are having an ideological debate. How do you judge who is right?", "By look at cold, hard empirical data and historical reference", "By assessing who has the more morally sound, empathetic approach", "sourceOfTruth"),

    // Phase 4
    new("If you worked on a movie set, what is your role?", "Production Manager (Budgets, logistics)", "Writer/Director (Creative vision, plot)", "outputFocus"),
    new("You have free time in the workshop/art room.", "Fixing an old, broken bicycle until it runs", "Sketching a futuristic, brand-new concept car", "outputFocus"),
    new("You are building a complex LEGO set.", "Follow the instruction manual page-by-page", "Look at the box photo and build a custom version", "environmentPreference"),
    new("Your group project partner ghosted you the night before it's due.", "Complete breakdown/panic", "Laser-focused execution under pressure", "stressResilience"),
    new("You are picked to lead a student committee.", "Backstage operator (Logistics, rules)", "Front-facing leader (Speeches, motivation)", "socialOrientation"),
    new("You are writing a major essay.", "Detailed bulleted outline first", "Stream of consciousness typing, organize later", "cognitivePace"),
    new("You have built a functional prototype app or project. What's your next move?", "Spend days polishing the UI, fixing minor alignment issues", "Deploy it instantly, flaws and all, to see if people like the core idea", "outputFocus"),

    // Phase 5
    new("You are playing a strategic video/board game.", "Slow, ultra-defensive, safe strategy", "High-risk, 80% fail rate for an instant win", "riskAppetite"),
    new("Your teammate makes a catastrophic mistake, ruining the game.", "Deep internal frustration at the ruined order", "Excitement because it just got chaotic", "stressResilience"),
    new("You are shopping for a new laptop or phone.", "Comparing numerical spec sheets/benchmarks", "Beautiful design, color, and user 'vibe'", "sourceOfTruth"),
    new("How do you manage your computer's desktop files?", "Strict, perfectly categorized folders", "Dump it all on the desktop and use the search bar", "environmentPreference"),
    new("You are reading a mystery book or watching a movie.", "I enjoy the ride at the author's pace", "I actively try to outsmart the plot and guess the ending", "cognitivePace"),
    new("A teacher assigns a concept your class hasn't covered yet.", "Skip it completely (Needs guardrails)", "Spend 30 minutes online trying to crack it (High risk/reward)", "riskAppetite"),

    // Phase 6
    new("You study a historic global economic crisis. What catches your focus?", "The mechanical timeline of events and specific legislative acts passed", "The underlying systemic flaws and psychological patterns of human greed", "abstractThinking"),
    new("You are finalizing a video edit or creative digital art piece.", "If the message is clear, it's done. I don't look closer.", "I will spend hours adjusting single frame jumps or individual color pixels", "detailFocus"),
    new("You are researching a topic online. What is your pattern?", "Open one tab, read it comprehensively from top to bottom before moving on", "Open 15 tabs simultaneously, rapidly scanning and jumping between insights", "cognitivePace"),
    new("You have to give an important class presentation.", "Write every word down verbatim on index cards and read them safely", "Bring basic bullet points and completely improvise the delivery on stage", "riskAppetite"),
    new("Your home internet crashes 1 hour before a massive online project submission.", "Instant existential dread and paralysis", "Cold, hyper-focused tactical triage (hotspotting, emailing alternatives)", "stressResilience"),
    new("You are playing a new open-world video game with no quest markers.", "Annoyed; I want an explicit mini-map directing my progression", "Thrilled; I prefer wandering without directions to discover my own path", "ambiguityTolerance"),

    // Phase 7 — Humanities Metrics
    new("You are reading two articles: one is packed with statistics, the other is a personal narrative. Which do you trust more?", "The data and statistics — facts don't lie", "The human story — context and lived experience matter more", "narrativeThinking"),
    new("A new law is being debated. How do you evaluate whether it's a good idea?", "By analyzing its measurable outcomes and economic data", "By asking whether it's fair and how it affects vulnerable people", "ethicalOrientation"),
    new("You are assigned a history project. What angle excites you most?", "Future implications — what this event changed going forward", "Deep context — understanding the world that made this event happen", "temporalFocus"),
    new("When you need to explain a complex idea, what feels most natural?", "Drawing a diagram, chart, or writing structured code/formulas", "Writing an essay, giving a speech, or crafting a metaphor", "expressionMode"),
    new("You are watching a documentary. Which topic keeps you glued to the screen?", "How one brilliant individual changed history through their mind", "How entire cultures, economies, or societies rise and collapse", "humanSystemsInterest"),
    new("You are editing someone else's writing. What do you notice first?", "Whether the argument is logically structured and factually accurate", "Whether the tone, word choice, and voice feel authentic and powerful", "languageAffinity"),
    new("Two people in your friend group are in a serious conflict. What's your instinct?", "Figure out who is objectively right based on what actually happened", "Get both sides talking and help them understand each other's feelings", "conflictApproach"),
    new("Why do you learn new things?", "To build something, solve a problem, or create a tangible result", "To understand the world better, even if it leads nowhere practical", "knowledgePursuit"),
    new("A philosopher claims that truth is different for every culture. Your reaction?", "That's dangerous relativism — truth must be objective and provable", "That's profound — reality is always filtered through human experience", "structureOfTruth"),
};

// 2. Endpoint to Serve the HTML Frontend Interactively
app.MapGet("/", () => Results.Content(GetHtmlContent(questions), "text/html"));

// 3. Endpoint to Process Responses and Query Gemini
app.MapPost("/api/evaluate", async (Dictionary<int, int> answers) =>
{
    var rawMetrics = questions.Select(q => q.MetricKey).Distinct().ToDictionary(k => k, k => 0);
    var metricMaxCounts = questions.GroupBy(q => q.MetricKey).ToDictionary(g => g.Key, g => g.Count());

    for (int i = 0; i < questions.Count; i++)
    {
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
        normalizedMetrics[metric.Key] = $"{percentage}%";
    }

    var apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY")
        ?? throw new InvalidOperationException("GEMINI_API_KEY is not set.");

    var client = new Client(apiKey: apiKey);
    
    var telemetryList = string.Join("\n", normalizedMetrics.Select(m => $"- {m.Key}: {m.Value}"));

    string systemPrompt = $$"""
        You are an elite career strategist, talent coach, and workforce futurist. Your task is to analyze a high school student's behavioral profile derived from a 49-question simulation.
        
        The provided telemetry data is normalized on a scale from -100% to +100%. 
        - A negative percentage indicates a strong preference for the Left Trait (e.g., structured, methodical, concrete, data-driven).
        - A positive percentage indicates a strong preference for the Right Trait (e.g., fluid, rapid, abstract, narrative-driven).
        - Near 0% indicates neutrality or adaptability.

        Based on this telemetry, you must:
        1. Deduce their "Big Five" (OCEAN) personality profile (Openness, Conscientiousness, Extraversion, Agreeableness, Neuroticism). Explain each briefly in the context of their scores.
        2. Generate exactly 3 distinct, accessible, and forward-looking career paths tailored to their unique profile.
        3. Generate a small list (4-6) of alternative, distinct "Other Favorable Jobs".
        4. Provide an empowering final synthesis.

        CRITICAL BIAS & DIVERSITY GUARDRAILS:
        - Absolute Zero Bias: Do not favor STEM or technology careers merely because the assessment exists as software. (however, if the telemetry strongly supports it, you can recommend them with a clear rationale).
        - Radical Diversity: The 3 core career recommendations must NOT be variations of the same industry. Ensure a cross-disciplinary mix based on the telemetry (e.g., if one is analytical/technical, another must be creative/humanities-driven, and another should be social-impact, trades, logistics, or educational).
        - Grounded Reality: Ensure these are accessible career paths (e.g., Teacher, Urban Planner, Electrician, Social Worker, Supply Chain Manager, Industrial Designer).

        Formatting & Style Rules:
        - Tone: Inspiring, analytical, accessible for a high schooler.
        - Perspective: Speak directly to them using "You" and "Your."
        - CRITICAL: Never quote raw numbers, percentages, or metric variables in your output. Translate the telemetry into descriptive natural language.
        
        YOU MUST RETURN YOUR RESPONSE AS A VALID JSON OBJECT EXACTLY MATCHING THIS SCHEMA:
        {
            "oceanProfile": {
                "openness": "String describing their Openness",
                "conscientiousness": "String describing their Conscientiousness",
                "extraversion": "String describing their Extraversion",
                "agreeableness": "String describing their Agreeableness",
                "neuroticism": "String describing their Neuroticism/Stress Resilience"
            },
            "careerPaths": [
                {
                    "title": "Job Title",
                    "whyThisFits": "Paragraph explaining why",
                    "workEnvironment": "Paragraph on where they will work",
                    "dayInTheLife": "Paragraph on daily tasks",
                    "challengeAndGrowth": "Paragraph on a potential hurdle and how to fix it"
                }
            ],
            "otherFavorableJobs": [ "Job A", "Job B", "Job C", "Job D", "Job E" ],
            "overallSynthesis": "A motivational concluding paragraph synthesizing their strengths"
        }

        Telemetry Data:
        {{telemetryList}}
        """;

    try
    {
        var response = await client.Models.GenerateContentAsync(
            model: "gemini-3.1-flash-lite", 
            contents: "Analyze my profile telemetry. Ensure zero bias, high career diversity, and generate my tailored JSON.",
            config: new GenerateContentConfig
            {
                SystemInstruction = new Content { Parts = new List<Part> { new() { Text = systemPrompt } } },
                Temperature = 0.8f,
                ResponseMimeType = "application/json" 
            }
        );

        string reportText = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                            ?? throw new Exception("No assessment report was generated by the AI engine.");

        return Results.Content(reportText, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = $"Engine communication error: {ex.Message}" }, statusCode: 500);
    }
});

// 4. Start the app dynamically based on the environment
var port = System.Environment.GetEnvironmentVariable("PORT");

if (string.IsNullOrEmpty(port))
{
    // Local Testing: Let standard .NET settings take over (localhost:5000/5001)
    app.Run(); 
}
else
{
    // Railway Deployment: Bind to their specific dynamic cloud port
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
        <title>Cognitive Routing Engine</title>
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
                    CAREER TRAJECTORY ASSESSMENT
                </h1>
                <p class="text-gray-400 text-xl font-medium font-sans">Adjust the sliders to reflect your interests and strengths.</p>
            </header>

            <form id="quizForm" class="space-y-8">
                {{listHtml}}
                <div class="text-center pt-8">
                    <button type="submit" id="submitBtn" class="w-full sm:w-auto bg-black hover:bg-gray-800 text-white font-bold px-12 py-5 rounded-xl shadow-lg transform transition active:scale-95 text-xl font-sans">
                        Compute My Trajectory
                    </button>
                </div>
            </form>

            <div id="outputContainer" class="hidden mt-12 space-y-8">
                <div id="loadingState" class="bg-white border border-gray-300 rounded-2xl p-10 shadow-2xl text-center">
                    <div class="animate-pulse flex flex-col items-center">
                        <div class="w-12 h-12 border-4 border-black border-t-transparent rounded-full animate-spin mb-4"></div>
                        <h2 class="text-2xl font-bold text-gray-800 font-sans">Analyzing Telemetry Vectors...</h2>
                        <p class="text-gray-500 mt-2">Deducing OCEAN traits, neutralizing bias, and scanning diverse sectors.</p>
                    </div>
                </div>

                <div id="resultsUI" class="hidden space-y-8">
                    <div class="bg-gray-50 border-l-8 border-black rounded-r-2xl p-8 shadow-lg">
                        <h2 class="text-3xl font-black text-black mb-6 font-sans uppercase">Your OCEAN Profile</h2>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 text-lg" id="oceanContent"></div>
                    </div>

                    <div id="careersContent" class="space-y-6"></div>

                    <div id="otherJobsContainer"></div>

                    <div class="bg-black text-white rounded-2xl p-8 shadow-2xl">
                        <h2 class="text-3xl font-black mb-4 font-sans uppercase">The Verdict</h2>
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
                btn.innerText = "Processing...";
                
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
                    
                    if(data.error) {
                        alert(data.error);
                        outContainer.classList.add('hidden');
                    } else {
                        renderResults(data);
                        loadingState.classList.add('hidden');
                        resultsUI.classList.remove('hidden');
                    }
                } catch (err) {
                    alert("Submission error occurred. Please check console.");
                    console.error(err);
                    outContainer.classList.add('hidden');
                } finally {
                    btn.disabled = false;
                    btn.classList.remove('opacity-50', 'cursor-not-allowed');
                    btn.innerText = "Compute My Trajectory";
                }
            });

            function renderResults(data) {
                const oceanHtml = `
                    <div><span class="font-bold text-gray-900 block">Openness</span> <span class="text-gray-700">${data.oceanProfile.openness}</span></div>
                    <div><span class="font-bold text-gray-900 block">Conscientiousness</span> <span class="text-gray-700">${data.oceanProfile.conscientiousness}</span></div>
                    <div><span class="font-bold text-gray-900 block">Extraversion</span> <span class="text-gray-700">${data.oceanProfile.extraversion}</span></div>
                    <div><span class="font-bold text-gray-900 block">Agreeableness</span> <span class="text-gray-700">${data.oceanProfile.agreeableness}</span></div>
                    <div class="md:col-span-2"><span class="font-bold text-gray-900 block">Neuroticism (Stress Resilience)</span> <span class="text-gray-700">${data.oceanProfile.neuroticism}</span></div>
                `;
                document.getElementById('oceanContent').innerHTML = oceanHtml;

                const careersHtml = data.careerPaths.map(career => `
                    <div class="bg-white border border-gray-300 rounded-2xl p-8 shadow-xl transition-all hover:shadow-2xl hover:-translate-y-1">
                        <h3 class="text-3xl font-bold text-black mb-6 font-sans border-b-2 border-gray-100 pb-4">${career.title}</h3>
                        <div class="space-y-4 text-lg text-gray-800">
                            <p><strong class="text-black block mb-1">Why This Fits You:</strong> ${career.whyThisFits}</p>
                            <p><strong class="text-black block mb-1">Where You Will Work:</strong> ${career.workEnvironment}</p>
                            <p><strong class="text-black block mb-1">A Day in Your Life:</strong> ${career.dayInTheLife}</p>
                            <div class="bg-red-50 text-red-900 p-4 rounded-xl mt-6 border border-red-100">
                                <strong class="block mb-1">The Challenge & The Growth Strategy:</strong>
                                ${career.challengeAndGrowth}
                            </div>
                        </div>
                    </div>
                `).join('');
                document.getElementById('careersContent').innerHTML = careersHtml;

                // Render Other Favorable Jobs
                if (data.otherFavorableJobs && data.otherFavorableJobs.length > 0) {
                    const otherJobsTags = data.otherFavorableJobs.map(job => 
                        `<span class="inline-block bg-gray-200 text-gray-900 px-4 py-2 rounded-full text-sm font-bold shadow-sm border border-gray-300">${job}</span>`
                    ).join('');
                    
                    document.getElementById('otherJobsContainer').innerHTML = `
                        <div class="bg-white border border-gray-300 rounded-2xl p-8 shadow-md">
                            <h3 class="text-2xl font-bold text-black mb-4 font-sans">Other Favorable Trajectories</h3>
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