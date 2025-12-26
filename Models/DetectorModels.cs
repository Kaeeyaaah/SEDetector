using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SEDetector.Models
{
    public class AnalysisRequest
    {
        // We use different properties for different tabs, or a generic input
        public string? InputText { get; set; }
        public string? InputUrl { get; set; }
        public IFormFile? Screenshot { get; set; }
        public string AnalysisType { get; set; } = "Text"; // "Text", "URL", "Screenshot"
    }

    public class AnalysisResult
    {
        public bool IsThreat { get; set; }
        public double ConfidenceScore { get; set; } // 0.0 to 1.0
        public string RiskLevel { get; set; } // Low, Medium, High
        public List<string> Flags { get; set; } = new List<string>();
        public string? ExtractedText { get; set; } // For OCR results
    }

    public class DashboardViewModel
    {
        public AnalysisRequest Request { get; set; }
        public AnalysisResult? Result { get; set; }
    }
}