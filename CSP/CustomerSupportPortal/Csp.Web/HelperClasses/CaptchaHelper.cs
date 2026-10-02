using BotDetect;
using BotDetect.Web;
using BotDetect.Web.UI;
using BotDetect.Web.UI.Mvc;
using System;

namespace Csp.Web.HelperClasses {
	public class CaptchaHelper {
		public static MvcCaptcha GetLoginCaptcha() {
			// create the control instance
			var loginCaptcha = new MvcCaptcha(
			"LoginCaptcha") { UserInputClientID = "CaptchaCode" };

			// all Captcha properties are set in this event handler
			loginCaptcha.InitializedCaptchaControl += LoginCaptcha_InitializedCaptchaControl;
			// all Captcha settings have to be saved before rendering
			loginCaptcha.SaveSettings();

			return loginCaptcha;
		}

		// event handler used for Captcha control property randomization
		static void LoginCaptcha_InitializedCaptchaControl(
				object sender, InitializedCaptchaControlEventArgs e) {
			if (e.CaptchaId != "LoginCaptcha") {
				return;
			}

			var loginCaptcha = sender as CaptchaControl;

			// fixed Captcha settings 
			if (loginCaptcha != null) {
				loginCaptcha.ImageSize = new System.Drawing.Size(200, 50);
				loginCaptcha.CodeLength = CaptchaRandomization.GetRandomCodeLength(4, 6);

				// randomized Captcha settings
				loginCaptcha.ImageStyle = CaptchaRandomization.GetRandomImageStyle();
				loginCaptcha.SoundStyle = CaptchaRandomization.GetRandomSoundStyle();
			}
		}
	}
}