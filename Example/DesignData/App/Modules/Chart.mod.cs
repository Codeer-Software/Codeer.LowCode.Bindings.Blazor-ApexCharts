
void DetailLayoutDesign_OnAfterInitialization()
{

    var a1 = new ChartAnnotation();
    a1.Axis = AnnotationAxis.X;
    a1.Label = "xxx";
    a1.Color = "#ff0000";
    a1.Value = 300;
    a1.IsDashed = true;
    ApexChart5.AddAnnotation("xxx", a1);
   // ApexChart5.AddAnnotation("yyy", AnnotationAxis.Y, 2.5, "#ff0000", "");
}