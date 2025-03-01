namespace Codeer.LowCode.Bindings.ApexCharts.Models
{
    public class ChartAnnotation
    {
        public AnnotationAxis Axis { get; set; }
        public double Value { get; set; }
        public string Color { get; set; } = "#00E396";
        public string? Label { get; set; }
        public bool IsDashed { get; set; }
    }
}
