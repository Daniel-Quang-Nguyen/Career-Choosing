using System;
using System.Collections.Generic;
using System.Linq;
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
    new(" Your morning alarm goes off. What does getting ready look like?", "Strict minute-by-minute routine", "Wing it based on how I feel", "environmentPreference"),
    new(" You are picking your outfit for school. What dictates your choice?", "Pure comfort, utility, and weather", "Aesthetics, expression, and the 'vibe'", "sourceOfTruth"),
    new(" You miss the bus or your ride is late. How do you react?", "Deep internal panic", "Instantly adapt and find a new solution", "stressResilience"),
    new(" You walk into a loud, crowded high school hallway.", "Put headphones in and avoid eye contact", "Look around to spot friends and say hi", "socialOrientation"),
    new(" You look at your locker or backpack organization.", "Everything is in exact, labeled folders", "Absolute chaos, but I know where things are", "environmentPreference"),
    new(" You read the morning announcements on the board.", "I just skim the main headline", "I immediately notice a spelling typo in the text", "detailFocus"),
    new(" You are listening to a podcast or music on the way to school. What catches your brain?", "Concrete, real-world true stories or news", "Deep, metaphorical concepts or complex lyrical storytelling", "abstractThinking"),

    // Phase 2
    new(" 1st Period gives you a major project with NO rubric.", "Panic / Tell me exactly what to do", "Freedom / I love making my own rules", "ambiguityTolerance"),
    new(" You have to learn a complex new software for a class.", "Read the manual / Watch a 30-min tutorial", "Click buttons randomly until it works", "cognitivePace"),
    new(" Which class discussion makes your brain light up?", "Calculating physical, concrete measurements", "Debating an abstract philosophical question", "abstractThinking"),
    new(" In History, you analyze an event from a perspective you disagree with.", "Frustrating / I prefer objective facts", "Effortless / I like exploring alternate views", "ambiguityTolerance"),
    new(" In Science lab, your experiment fails the textbook result.", "Frustrated the lab is ruined", "Curious to debug the rogue variables", "riskAppetite"),
    new(" You are solving a complex math equation.", "Follow the clean, orderly steps exactly", "Hunt for an unorthodox shortcut", "cognitivePace"),
    new(" You are reviewing peer-reviewed study sheets.", "I trust the summary concept entirely", "I cross-reference the data points and indices to ensure no metrics are missed", "detailFocus"),

    // Phase 3
    new(" It's lunchtime. Where do you recharge?", "Quiet library alone or with one person", "The loudest, most crowded table", "socialOrientation"),
    new(" A friend is crying over a major personal problem.", "Analyze the root cause and map out a fix", "Focus entirely on comforting and validating them", "sourceOfTruth"),
    new(" You and your friends plan a weekend hangout.", "Strict itinerary of where to go and when", "Just show up and figure it out", "environmentPreference"),
    new(" Your friend group suddenly changes plans 5 minutes before meeting.", "I hate last-minute disruptions", "I don't mind, go with the flow", "stressResilience"),
    new(" A system or project you are working on breaks.", "Pull up the logs and fix it solo in silence", "Call a group chat to brainstorm out loud", "socialOrientation"),
    new(" You hear a rumor or half-story about some school drama.", "I need to know the exact factual truth immediately", "I'm comfortable not knowing the full context", "ambiguityTolerance"),
    new(" Two friends are having an ideological debate. How do you judge who is right?", "By look at cold, hard empirical data and historical reference", "By assessing who has the more morally sound, empathetic approach", "sourceOfTruth"),

    // Phase 4
    new(" If you worked on a movie set, what is your role?", "Production Manager (Budgets, logistics)", "Writer/Director (Creative vision, plot)", "outputFocus"),
    new(" You have free time in the workshop/art room.", "Fixing an old, broken bicycle until it runs", "Sketching a futuristic, brand-new concept car", "outputFocus"),
    new(" You are building a complex LEGO set.", "Follow the instruction manual page-by-page", "Look at the box photo and build a custom version", "environmentPreference"),
    new(" Your group project partner ghosted you the night before it's due.", "Complete breakdown/panic", "Laser-focused execution under pressure", "stressResilience"),
    new(" You are picked to lead a student committee.", "Backstage operator (Logistics, rules)", "Front-facing leader (Speeches, motivation)", "socialOrientation"),
    new(" You are writing a major essay.", "Detailed bulleted outline first", "Stream of consciousness typing, organize later", "cognitivePace"),
    new(" You have built a functional prototype app or project. What's your next move?", "Spend days polishing the UI, fixing minor alignment issues", "Deploy it instantly, flaws and all, to see if people like the core idea", "outputFocus"),

    // Phase 5
    new(" You are playing a strategic video/board game.", "Slow, ultra-defensive, safe strategy", "High-risk, 80% fail rate for an instant win", "riskAppetite"),
    new(" Your teammate makes a catastrophic mistake, ruining the game.", "Deep internal frustration at the ruined order", "Excitement because it just got chaotic", "stressResilience"),
    new(" You are shopping for a new laptop or phone.", "Comparing numerical spec sheets/benchmarks", "Beautiful design, color, and user 'vibe'", "sourceOfTruth"),
    new(" How do you manage your computer's desktop files?", "Strict, perfectly categorized folders", "Dump it all on the desktop and use the search bar", "environmentPreference"),
    new(" You are reading a mystery book or watching a movie.", "I enjoy the ride at the author's pace", "I actively try to outsmart the plot and guess the ending", "cognitivePace"),
    new(" A teacher assigns a concept your class hasn't covered yet.", "Skip it completely (Needs guardrails)", "Spend 30 minutes online trying to crack it (High risk/reward)", "riskAppetite"),

    // Phase 6
    new(" You study a historic global economic crisis. What catches your focus?", "The mechanical timeline of events and specific legislative acts passed", "The underlying systemic flaws and psychological patterns of human greed", "abstractThinking"),
    new(" You are finalizing a video edit or creative digital art piece.", "If the message is clear, it's done. I don't look closer.", "I will spend hours adjusting single frame jumps or individual color pixels", "detailFocus"),
    new(" You are researching a topic online. What is your pattern?", "Open one tab, read it comprehensively from top to bottom before moving on", "Open 15 tabs simultaneously, rapidly scanning and jumping between insights", "cognitivePace"),
    new(" You have to give an important class presentation.", "Write every word down verbatim on index cards and read them safely", "Bring basic bullet points and completely improvise the delivery on stage", "riskAppetite"),
    new(" Your home internet crashes 1 hour before a massive online project submission.", "Instant existential dread and paralysis", "Cold, hyper-focused tactical triage (hotspotting, emailing alternatives)", "stressResilience"),
    new(" You are playing a new open-world video game with no quest markers.", "Annoyed; I want an explicit mini-map directing my progression", "Thrilled; I prefer wandering without directions to discover my own path", "ambiguityTolerance"),

    // Phase 7 — Humanities Metrics
    new(" You are reading two articles: one is packed with statistics, the other is a personal narrative. Which do you trust more?", "The data and statistics — facts don't lie", "The human story — context and lived experience matter more", "narrativeThinking"),
    new(" A new law is being debated. How do you evaluate whether it's a good idea?", "By analyzing its measurable outcomes and economic data", "By asking whether it's fair and how it affects vulnerable people", "ethicalOrientation"),
    new(" You are assigned a history project. What angle excites you most?", "Future implications — what this event changed going forward", "Deep context — understanding the world that made this event happen", "temporalFocus"),
    new(" When you need to explain a complex idea, what feels most natural?", "Drawing a diagram, chart, or writing structured code/formulas", "Writing an essay, giving a speech, or crafting a metaphor", "expressionMode"),
    new(" You are watching a documentary. Which topic keeps you glued to the screen?", "How one brilliant individual changed history through their mind", "How entire cultures, economies, or societies rise and collapse", "humanSystemsInterest"),
    new(" You are editing someone else's writing. What do you notice first?", "Whether the argument is logically structured and factually accurate", "Whether the tone, word choice, and voice feel authentic and powerful", "languageAffinity"),
    new(" Two people in your friend group are in a serious conflict. What's your instinct?", "Figure out who is objectively right based on what actually happened", "Get both sides talking and help them understand each other's feelings", "conflictApproach"),
    new(" Why do you learn new things?", "To build something, solve a problem, or create a tangible result", "To understand the world better, even if it leads nowhere practical", "knowledgePursuit"),
    new(" A philosopher claims that truth is different for every culture. Your reaction?", "That's dangerous relativism — truth must be objective and provable", "That's profound — reality is always filtered through human experience", "structureOfTruth"),
};

// 2. Endpoint to Serve the HTML Frontend Interactively
app.MapGet("/", () => Results.Content(GetHtmlContent(questions), "text/html"));

// 3. Endpoint to Process Responses and Query Gemini
app.MapPost("/api/evaluate", async (Dictionary<int, int> answers) =>
{
    var metrics = new Dictionary<string, int>
    {
        { "ambiguityTolerance", 0 }, { "socialOrientation", 0 }, { "cognitivePace", 0 },
        { "riskAppetite", 0 }, { "stressResilience", 0 }, { "detailFocus", 0 },
        { "abstractThinking", 0 }, { "sourceOfTruth", 0 }, { "outputFocus", 0 },
        { "environmentPreference", 0 }, { "narrativeThinking", 0 }, { "ethicalOrientation", 0 },
        { "temporalFocus", 0 }, { "expressionMode", 0 }, { "humanSystemsInterest", 0 },
        { "languageAffinity", 0 }, { "conflictApproach", 0 }, { "knowledgePursuit", 0 },
        { "structureOfTruth", 0 }
    };

    for (int i = 0; i < questions.Count; i++)
    {
        if (answers.TryGetValue(i, out int baselineScore))
        {
            metrics[questions[i].MetricKey] += (baselineScore - 5);
        }
    }

    var apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY")
        ?? throw new InvalidOperationException("GEMINI_API_KEY is not set.");

    var client = new Client(apiKey: apiKey);
    string systemPrompt = $"""
        You are an elite career strategist, talent coach, and workforce futurist. Your task is to analyze a high school student's behavioral profile, problem-solving style, and strengths derived from a recent 49-question behavioral simulation.

    Provide exactly 3 distinct, specialized, and forward-looking career paths tailored to their unique profile. Avoid generic, broad-category jobs (like "Doctor" or "Lawyer"). Instead, identify specific, modern, or emerging niches across various industries—ranging from technology and science to the arts, humanities, and social impact fields—that align with their psychological makeup and cognitive style.
    Do not be too specific and obscure that the career path doesn't actually exist or isn't realistically attainable with a high school diploma and some college education. Each career path should be something they can realistically pursue with dedication, passion, and the right educational choices over the next 10 years.
    Avoid being biased towards science and technology careers. Consider creative, artistic, social impact, humanities, and interdisciplinary fields that might better suit their profile. The more diverse the three career paths are from each other, the better.
    Formatting & Style Rules:
    - Tone: Inspiring, analytical, and sophisticated, yet highly accessible and motivating for an ambitious high school student.
    - Perspective: Speak directly to them using "You" and "Your."
    - Structure: Use clear markdown headings, bullet points, and horizontal rules to separate the career paths.
    - Visuals: Bold the career names and key psychological strengths.
    - CRITICAL: Do not mention raw numbers, scores, or metric values. Translate all data into descriptive, natural language (e.g., say "your natural empathy and collaborative drive" instead of "your teamwork score of 90%").

    For Each Career Path, Provide:

    ## **[Insert Specialized Career Title]**

    *   **The Blueprint (Why This Fits You):** Connect their unique behavioral traits, natural talents, and problem-solving styles into a compelling narrative explaining why they would thrive in this niche. 
    *   **A Day in Your Life:** Paint a vivid, exciting picture of what their day-to-day work, environment, and projects would actually look like.
    *   **The Challenge & The Growth Strategy:** Identify one potential hurdle they might face in this role based on their personality, and give them a concrete strategy to overcome it.
    ---
    Student Profile Multi-Vector Telemetry Data (use descriptively, never quote these numbers):
    - Comfort with Ambiguity: {metrics["ambiguityTolerance"]} (Higher = thrives in chaos; Lower = needs rules)
    - Social Orientation: {metrics["socialOrientation"]} (Negative = Independent/Backstage; Positive = Collaborative/Public)
    - Cognitive Pace: {metrics["cognitivePace"]} (Negative = Methodical/Deliberate; Positive = Rapid/Trial-and-Error)
    - Risk Appetite: {metrics["riskAppetite"]} (Negative = Safe/Predictable; Positive = High-Risk/Experimental)
    - Stress Resilience: {metrics["stressResilience"]} (Negative = Requires structure; Positive = Thrives under pressure)
    - Detail Focus: {metrics["detailFocus"]} (Negative = Big-Picture Visionary; Positive = Granular Quality Control)
    - Abstract Thinking: {metrics["abstractThinking"]} (Negative = Practical/Concrete; Positive = Theoretical/Abstract)
    - Source of Truth: {metrics["sourceOfTruth"]} (Negative = Data-Driven/Specs; Positive = Intuitive/Empathetic)
    - Output Focus: {metrics["outputFocus"]} (Negative = Optimizer/Organizer; Positive = Builder/Creative Visionary)
    - Environment Preference: {metrics["environmentPreference"]} (Negative = Structured/Guardrails; Positive = Fluid/Dynamic)
    - Narrative Thinking: {metrics["narrativeThinking"]} (Negative = Data/Logic driven; Positive = Story/Meaning driven)
    - Ethical Orientation: {metrics["ethicalOrientation"]} (Negative = Rule-based/Consequentialist; Positive = Empathy-based/Relational)
    - Temporal Focus: {metrics["temporalFocus"]} (Negative = Future-oriented/Innovative; Positive = Historically/Contextually grounded)
    - Expression Mode: {metrics["expressionMode"]} (Negative = Visual/Numerical/Spatial; Positive = Verbal/Written/Rhetorical)
    - Human Systems Interest: {metrics["humanSystemsInterest"]} (Negative = Individual psychology; Positive = Societal/Cultural systems)
    - Language Affinity: {metrics["languageAffinity"]} (Negative = Logic/Structure focused; Positive = Tone/Voice/Nuance focused)
    - Conflict Approach: {metrics["conflictApproach"]} (Negative = Objective/Analytical resolution; Positive = Dialogue/Mediation focused)
    - Knowledge Pursuit: {metrics["knowledgePursuit"]} (Negative = Applied/Practical learning; Positive = Interpretive/Philosophical learning)
    - Structure of Truth: {metrics["structureOfTruth"]} (Negative = Objective/Empirical; Positive = Relativist/Contextual)
    """;

    try
    {
        var response = await client.Models.GenerateContentAsync(
            model: "gemini-3.1-flash-lite",
            contents: "Analyze my profile telemetry and generate my 3 tailored career paths.",
            config: new GenerateContentConfig
            {
                SystemInstruction = new Content { Parts = new List<Part> { new() { Text = systemPrompt } } },
                Temperature = 0.7f
            }
        );

        string reportText = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                            ?? "No assessment report was generated by the AI engine. Please check inputs.";

        return Results.Json(new { report = reportText });
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = $"Engine communication error: {ex.Message}" }, statusCode: 500);
    }
});

var port = System.Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");

// 4. Global View Engine Generation Function
string GetHtmlContent(List<Question> questionSet)
{
    var listHtml = string.Join("\n", questionSet.Select((q, index) => $$"""
        <div class="question-card flex flex-col border-2 border-gray-400 rounded-lg overflow-hidden mb-10 shadow-lg bg-white">
            
            <div class="bg-white text-black p-8 text-center border-b-2 border-gray-400">
                <h3 class="text-3xl">{{index + 1}}. {{q.Text}}</h3>
            </div>
            
            <div class="flex w-full border-b-2 border-gray-400">
                <div class="w-1/2 bg-white text-black p-10 text-center flex items-center justify-center">
                    <span class="text-xl">{{q.LowLabel}}</span>
                </div>
                <div class="w-1/2 bg-[#1a1a1a] text-white p-10 text-center flex items-center justify-center">
                    <span class="text-xl">{{q.HighLabel}}</span>
                </div>
            </div>

            <div class="flex w-full relative">
                <div class="absolute inset-0 flex">
                    <div class="w-1/2 bg-white"></div>
                    <div class="w-1/2 bg-[#1a1a1a]"></div>
                </div>
                
                <div class="relative z-10 flex w-full items-center py-6 px-8">
                    <span class="text-5xl font-bold text-black mr-6">1</span>
                    
                    <input type="range" min="1" max="10" value="5.5" step="any" name="q_{{index}}" class="custom-slider w-full mx-2">
                    
                    <span class="text-5xl font-bold text-white ml-6">10</span>
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
        <script src="https://cdn.jsdelivr.net/npm/marked/marked.min.js"></script>
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
                <p class="text-gray-400 text-xl font-medium font-sans">Discover your hyper-specialized future path using predictive AI metrics.</p>
            </header>

            <form id="quizForm" class="space-y-8">
                {{listHtml}}
                <div class="text-center pt-8">
                    <button type="submit" class="w-full sm:w-auto bg-black hover:bg-gray-800 text-white font-bold px-12 py-5 rounded-xl shadow-lg transform transition active:scale-95 text-xl font-sans">
                        Show Results
                    </button>
                </div>
            </form>

            <div id="outputContainer" class="hidden mt-12 bg-white border border-gray-300 rounded-2xl p-6 sm:p-10 shadow-2xl">
                <h2 class="text-3xl font-bold border-b border-gray-300 pb-4 text-black mb-6">Your Personal Trajectory Assessment</h2>
                <div id="markdownOutput" class="prose max-w-none space-y-4 text-gray-800 text-lg"></div>
            </div>
        </div>

        <script>
            document.getElementById('quizForm').addEventListener('submit', async (e) => {
                e.preventDefault();
                const btn = e.target.querySelector('button');
                const outContainer = document.getElementById('outputContainer');
                const mdOut = document.getElementById('markdownOutput');
                
                btn.disabled = true;
                btn.innerText = "Analyzing Telemetry Vectors...";
                
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
                    } else {
                        outContainer.classList.remove('hidden');
                        mdOut.innerHTML = marked.parse(data.report);
                        outContainer.scrollIntoView({ behavior: 'smooth' });
                    }
                } catch (err) {
                    alert("Submission error occurred.");
                } finally {
                    btn.disabled = false;
                    btn.innerText = "Compute My Trajectory";
                }
            });
        </script>
    </body>
    </html>
    """;
}

// 5. C# Type Definitions
public record Question(string Text, string LowLabel, string HighLabel, string MetricKey);