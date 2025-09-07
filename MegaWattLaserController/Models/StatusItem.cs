namespace LaserControllerApp.Models
{
    public partial class StatusItem
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "--";
        public bool IsWarning { get; set; } = false;
        public double ProgressValue { get; set; } = 0;
    }
}
