using Microsoft.EntityFrameworkCore;

namespace JWTLearning.Models {

    // Why Owned Data Annotation?
    // that show this table [ RefreshToken ] that related to Application User Table, so that the resone why we ub set it in dbContext Model    
    [Owned] 
    public class RefreshToken {
        
        // refresh token property
        public string Token { get; set; }

        public DateTime ExpiresOn { get; set; }

        // => goes to
        public bool IsExpire => DateTime.UtcNow >= ExpiresOn;
        
        // that show creating time to refresh token!
        public DateTime CreateOn { get; set; }

        // that show canceling time to token!
            // this action be when user logout, change password, hacking process, any thing else
        public DateTime RevokedOn { get; set; }

        //to check if token still active or not!
        public bool isActive => RevokedOn == null && !IsExpire;
    }
}
