# tinder-function-app
An automated Tinder app running as an Azure Function in the cloud.

Running at: https://tinderfunctionapp.azurewebsites.net/

## Auto like and super like

The app runs in Azure at specific intervals, auto liking and super liking recommendations from Tinder.

## Match notification by e-mail

The app gets recent updates from Tinder, checks with Azure Table storage if there has been a new match recently then notifies the user by e-mail, including all photos of the matched person in the e-mail.

## How to generate a new Facebook token if it expires

The app authenticates with Tinder using a Facebook ID (which can be empty) and token (required). If the Facebook token expires, there needs to be generated a new token, otherwise a <code>401 Unauthorized</code> is returned from the Tinder API.

Here are the steps for generating a new Facebook token:

1. Open up a browser, preferably Chrome.
2. Press <code>F12</code> to open up the dev console.
3. Click on Network.
4. Navigate to the following URL (while the dev console is open):

https://www.facebook.com/v2.6/dialog/oauth?redirect_uri=fb464891386855067%3A%2F%2Fauthorize%2F&display=touch&state=%7B%22challenge%22%3A%22IUUkEUqIGud332lfu%252BMJhxL4Wlc%253D%22%2C%220_auth_logger_id%22%3A%2230F06532-A1B9-4B10-BB28-B29956C71AB1%22%2C%22com.facebook.sdk_client_state%22%3Atrue%2C%223_method%22%3A%22sfvc_auth%22%7D&scope=user_birthday%2Cuser_photos%2Cuser_education_history%2Cemail%2Cuser_relationship_details%2Cuser_friends%2Cuser_work_history%2Cuser_likes&response_type=token%2Csigned_request&default_audience=friends&return_scopes=true&auth_type=rerequest&client_id=464891386855067&ret=login&sdk=ios&logger_id=30F06532-A1B9-4B10-BB28-B29956C71AB1&ext=1470840777&hash=AeZqkIcf-NEW6vBd

5. You will be asked to allow Tinder access to your Facebook account, log in if you haven't done that already or click OK.
6. Look at the requests inside Network, click on the <code>confirm</code> request.
7. Click on Response.
8. You will see a long string, search for "access_token=" and then copy the token string until it ends just before "&expires_in".
9. Use this token to authenticate.

Note: As of today I have not found a way of automating the Facebook token generation upon expiration. Pull-requests are most welcome.

## Tinder API documentation sources

https://github.com/fbessez/Tinder
<br/>
https://gist.github.com/rtt/10403467