using SEDetector.Models;
using System.Text.RegularExpressions;
using Tesseract; 

namespace SEDetector.Services
{
    public class DetectorService
    {
        private readonly string _tessDataPath;

        public DetectorService(IWebHostEnvironment env)
        {
            // This points to the "tessdata" folder in your project root
            _tessDataPath = Path.Combine(env.ContentRootPath, "tessdata");
        }

        // TEXT ANALYSIS
        public AnalysisResult AnalyzeText(string text)
        {
            var result = new AnalysisResult();

            // Expanded triggers list
            var triggers = new[] {
                "urgent", "password", "verify", "suspend", "bank",
                "account locked", "unauthorized", "winner", "lottery",
                "kindly do the needful", "immediate action", "click here"
            };

            int count = triggers.Count(t => text.ToLower().Contains(t));

            result.ConfidenceScore = count * 0.2;
            if (result.ConfidenceScore > 1.0) result.ConfidenceScore = 1.0;

            result.IsThreat = result.ConfidenceScore > 0.5;
            result.RiskLevel = result.IsThreat ? "High" : "Low";

            if (result.IsThreat) result.Flags.Add("Urgency or sensitive keywords detected.");

            return result;
        }

        // URL ANALYSIS
        public AnalysisResult AnalyzeUrl(string url)
        {
            var result = new AnalysisResult();

            if (url.Length > 75) result.Flags.Add("Suspiciously long URL");

            string[] safeDomains = { "paypal.com", "google.com", "facebook.com", "microsoft.com" };
            Uri uri;
            try
            {
                uri = new Uri(url.StartsWith("http") ? url : "http://" + url);
                string host = uri.Host.Replace("www.", "");

                foreach (var safe in safeDomains)
                {
                    int distance = ComputeLevenshteinDistance(host, safe);
                    if (distance > 0 && distance <= 2)
                    {
                        result.IsThreat = true;
                        result.Flags.Add($"Typosquatting detected: resembles {safe}");
                        result.ConfidenceScore = 0.9;
                    }
                }
            }
            catch { result.Flags.Add("Invalid URL format"); }

            result.RiskLevel = result.IsThreat ? "High" : "Low";
            return result;
        }

        // SCREENSHOT ANALYSIS (Actual Tesseract Implementation)
        public async Task<AnalysisResult> AnalyzeImageAsync(IFormFile image)
        {
            string extractedText = "";

            try
            {
                // Convert the uploaded image file into a byte array
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();

                    // Initialize the Tesseract Engine
                    // "eng" = English language
                    using (var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default))
                    {
                        using (var img = Pix.LoadFromMemory(fileBytes))
                        {
                            using (var page = engine.Process(img))
                            {
                                extractedText = page.GetText();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                extractedText = $"Error reading image: {ex.Message}. Did you create the 'tessdata' folder?";
            }

            // Reuse the text analysis logic on the extracted text
            var result = AnalyzeText(extractedText);

            // Store the text so the user can see what the OCR read
            result.ExtractedText = extractedText;

            return result;
        }

        // Helper: Levenshtein Algorithm
        private int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}