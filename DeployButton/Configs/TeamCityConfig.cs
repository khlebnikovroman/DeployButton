namespace DeployButton
{
    public class TeamCityConfig
    {
        public string BaseUrl { get; set; } = "http://localhost:8111";
        public string BuildConfigurationId { get; set; } = "MyProject_Build";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}