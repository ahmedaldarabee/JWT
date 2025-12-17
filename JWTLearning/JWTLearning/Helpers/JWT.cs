namespace JWTLearning.Helpers {

    public class JWT {
        protected string Key { get; set; }
        
        protected string Issuer { get; set; }
        
        protected string Audience { get; set; }

        protected string DurationInDays { get; set; }

    }
}
