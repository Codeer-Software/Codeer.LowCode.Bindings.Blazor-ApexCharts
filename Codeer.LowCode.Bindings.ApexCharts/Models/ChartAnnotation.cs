namespace Codeer.LowCode.Bindings.ApexCharts.Models
{
    public class ChartAnnotation
    {
        public AnnotationAxis Axis { get; set; }
        public object Value { get; set; } = 0;
        public string Color { get; set; } = "#00E396";
        public string? Label { get; set; }
        public bool IsDashed { get; set; }
    }
}
