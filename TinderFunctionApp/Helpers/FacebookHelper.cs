using System.Threading;
using SimpleBrowser;

namespace TinderFunctionApp.Helpers
{
    public static class FacebookHelper
    {
        private static readonly string URL = "https://www.facebook.com/v2.6/dialog/oauth?redirect_uri=fb464891386855067%3A%2F%2Fauthorize%2F&display=touch&state=%7B%22challenge%22%3A%22IUUkEUqIGud332lfu%252BMJhxL4Wlc%253D%22%2C%220_auth_logger_id%22%3A%2230F06532-A1B9-4B10-BB28-B29956C71AB1%22%2C%22com.facebook.sdk_client_state%22%3Atrue%2C%223_method%22%3A%22sfvc_auth%22%7D&scope=user_birthday%2Cuser_photos%2Cuser_education_history%2Cemail%2Cuser_relationship_details%2Cuser_friends%2Cuser_work_history%2Cuser_likes&response_type=token%2Csigned_request&default_audience=friends&return_scopes=true&auth_type=rerequest&client_id=464891386855067&ret=login&sdk=ios&logger_id=30F06532-A1B9-4B10-BB28-B29956C71AB1&ext=1470840777&hash=AeZqkIcf-NEW6vBd";
        private static readonly string USER_AGENT = "Mozilla/5.0 (Linux; U; en-gb; KFTHWI Build/JDQ39) AppleWebKit/535.19 (KHTML, like Gecko) Silk/3.16 Safari/535.19";
        private static readonly Browser _browser = new Browser();

        public static string GetFbToken(string email, string password)
        {
            _browser.UserAgent = USER_AGENT;
            _browser.Navigate(URL);

            if (!_browser.CurrentHtml.Contains("__CONFIRM__"))
            {
                // Enters e-mail, password and logs in
                _browser.Find(ElementType.TextField, FindBy.Name, "email").Value = email;
                _browser.Find(ElementType.TextField, FindBy.Name, "pass").Value = password;
                _browser.Find(ElementType.Button, FindBy.Name, "login").Click(); 
            }
            
            // If two-factor auth is enabled, user gets a notification on phone and has to click ok
            while (!_browser.CurrentHtml.Contains("__CONFIRM__"))
            {
                _browser.Navigate(_browser.Url);
                Thread.Sleep(1000);
            }
            
            // Tinder is already authorized prompt, presses ok
            _browser.Find(ElementType.Button, FindBy.Name, "__CONFIRM__").Click();
            
            // Cut out the token from the html
            var indexFrom = _browser.CurrentHtml.IndexOf("access_token=") + "access_token=".Length;
            var indexTo = _browser.CurrentHtml.IndexOf("&", indexFrom);
            return _browser.CurrentHtml.Substring(indexFrom, indexTo - indexFrom);
        }
    }
}
