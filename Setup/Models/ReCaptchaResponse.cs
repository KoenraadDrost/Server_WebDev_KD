namespace Setup.Models
{
    public class ReCaptchaResponse
    {
        public bool success { get; set; }
        public DateTime challenge_ts { get; set; }
        public double score {  get; set; }

    }
}
