#NetAuction: Our AI-Driven Dev Journey

 For this project, our mission was clear: use as much AI as possible throughout the entire software development lifecycle—from the first line of implementation all the way through our CI/CD pipelines and cloud deployment. 

NetAuction is built on ASP.NET Core MVC (C#) with a React frontend, real-time notifications via SignalR, and two actual AI agents integrated directly into the product.

**this project is still under work. The first version is fine, but we have several errors to correct and improve our logic and project functionalities.**

## The AI Toolbelt

*   ChatGPT 
*   Claude 
*   Google Gemini

## AI as a Feature

We didn't just use AI to write code; we made it a core feature by building two agents in our `AiController`:

1.  **The Price Evaluator (Evaluator Preț):** This agent suggests a starting bid based on a product's title. We set it to run rigidly (temperature: 0.1) and told it to return *only* an integer. Because AI can be stubborn and sometimes returns markdown anyway, we added a defensive Regex (`Regex.Match(..., @"\d+")`) to extract just the number.
2.  **The Shopping Guide (Ghid Achiziții):** This is a creative consultant (temperature: 0.7). Unlike the evaluator, this agent queries our active database first to find items matching the user's budget and category, then feeds that real data to Gemini to generate HTML recommendations.

## From Implementation to CI/CD & Deployment

We treated AI as a team member within a strict engineering workflow, taking us from local development to our CI/CD and deployment processes:
*   **Modern Workflows:** We tracked everything via GitHub Issues and managed integrations through Pull Requests (PRs). Even AI-generated code had to pass peer code reviews before being merged.
*   **Tackling Tricky Implementations:** When building our 10-second `AuctionWorker` background service to close expired auctions, ChatGPT caught a classic trap: trying to access a scoped `DbContext` from a singleton service without using `IServiceProvider.CreateScope()`. 
*   **Frontend & Real-Time:** Copilot generated the React hooks (`useState`, `useEffect`, `useContext`) for our UI, while AI tutorials helped us wire up SignalR to push real-time outbid notifications to the browser while simultaneously firing off an email. 
*   **Incremental Progress:** Our EF Core migration history is a testament to our step-by-step CI/CD approach, moving smoothly from initial creation to adding features like user suspension, image uploads, and notifications.

## The Reality Check: AI is Not Flawless

As much as AI sped up our onboarding and cut down on boilerplate, it wasn't perfect. We ran into a few roadblocks:
*   **Stubborn Output:** Even with strict prompts, Gemini sometimes returned extra text, forcing us to write defensive regex.
*   **Hallucinated APIs:** Occasionally, the AI would suggest methods that didn't exist in our library versions, requiring us to double-check the official docs.
*   **Context Blindness:** AI couldn't see our entire repo, so we often had to manually adapt its code to fit our Romanian naming conventions (like `Licitatie`, `seller_id`, and `PretCurent`).
