using UnityEngine;
using System.Collections;

/// Google analytics class v 1.2
/// Optimized for .Net 1.1 and Unity iOS by Andreas Zeitler
/// Check in with my work at: www.azeitler.com, azeitler@dopanic.com
///
/// based on v1.0 (c) by Almar Joling
/// E-mail: unitydev@persistentrealities.com
/// Website: http://www.persistentrealities.com

/// Performs a google analytics request with given  parameters
/// Important: account id and domain id are required.
/// Make sure that your site is verified, so that tracking is enabled! (it needs a file to be placed somewhere in a directory online)

/// Example usage:
/// One time initialize:
/// Use a different hostname if you like.
/// GoogleAnalyticsHelper.Settings("UA-yourid", "127.0.0.1");

/// Then, for an "event":
///  GoogleAnalyticsHelper.LogEvent("levelname", "pickups", "pickupname", "optionallabel", optionalvalue);

/// For a "page" (like loading a level?)
/// GoogleAnalyticsHelper.LogPage("Mainscreen");


public class GoogleAnalyticsHelper
{
	private static string accountid;
	private static string domain;

	private static readonly GoogleAnalyticsHelper instance = new GoogleAnalyticsHelper();

	/// Init class with given site id and domain name
	public static void Settings(string p_accountid, string p_domain)
	{
		domain = p_domain;
		accountid = p_accountid;
	}


	public static void LogPage(string page)
	{
		LogEvent(page, "", "", "", 0);
	}

	/// Perform a log call, if only page is specified, a page visit will be tracked
	/// With category, action, and optionally opt_label and opt_value, there will be an event added instead
	/// Note that the statistics can take up to 24h before showing up at your Google Analytics account!
	private static Hashtable requestParams = new Hashtable();
	public static void LogEvent(string page, string category, string action, string opt_label, int opt_value)
        {
            if (domain == "" || domain.Length == 0)
            {
                Debug.Log("GoogleAnalytics settings not set!");
                return;
            }
           
            long utCookie = Random.Range(10000000,99999999);
            long utRandom = Random.Range(1000000000,2000000000);
            long utToday = GetEpochTime();
            string encoded_equals = "%3D";
            string encoded_separator = "%7C";
           
            string _utma = utCookie + "." + utRandom + "." + utToday + "." + utToday + "." + utToday + ".2" + WWW.EscapeURL (";") + WWW.EscapeURL ("+");
            string cookieUTMZstr = "utmcsr" + encoded_equals + "(direct)" + encoded_separator + "utmccn"+ encoded_equals +"(direct)" + encoded_separator + "utmcmd" + encoded_equals + "(none)" + WWW.EscapeURL (";");
           
            string _utmz = utCookie + "." + utToday + "2.2.2." + cookieUTMZstr;
           
            /// If no page was given, use levelname:
            if (page.Length == 0)
            {
                page = Application.loadedLevelName;
            }
     
            requestParams.Clear();
            requestParams.Add("utmwv", "4.6.5");           
            requestParams.Add("utmn", utRandom.ToString());
            requestParams.Add("utmhn", WWW.EscapeURL(domain));
            requestParams.Add("utmcs", "ISO-8859-1");
            requestParams.Add("utmsr", Screen.currentResolution.width.ToString() + "x" + Screen.currentResolution.height.ToString());  
           
            requestParams.Add("utmsc", "24-bit");
            requestParams.Add("utmul", "nl");  
            requestParams.Add("utmje", "0");       
            requestParams.Add("utmfl", "-");       
            requestParams.Add("utmdt", WWW.EscapeURL(page));
            requestParams.Add("utmhid", utRandom.ToString());              
            requestParams.Add("utmr", "-");            
            requestParams.Add("utmp", page);   
            requestParams.Add("utmac", accountid);     
            requestParams.Add("utmcc", "__utma" + encoded_equals +_utma + "__utmz" + encoded_equals + _utmz );
           
           
            /// Add event if available:
            if (category.Length > 0 && action.Length > 0)
            {
                string eventparams = "5(" + category + "*" + action;
               
                if (opt_label.Length > 0)
                {
                    eventparams += "*" + opt_label + ")(" + opt_value.ToString() + ")";
               
                } else {
                           
                    eventparams += ")";
                }
               
                requestParams.Add("utme", eventparams);
                requestParams.Add("utmt", "event");
            }
           
     
            /// Create query string:
            ArrayList pageURI = new ArrayList();
            foreach( string key in requestParams.Keys )
            {
                    pageURI.Add( key  + "=" + requestParams[key] + "&") ;
            }
           
            string url =   "http://www.google-analytics.com/__utm.gif?" + string.Join("", (string [])pageURI.ToArray(typeof(string)));
           
            /// Log url:
            //Debug.Log("[Google URL]" + url);       
           
            /// Execute query:
            WWW w = new WWW(url);
        }

	private static long GetEpochTime()
	{
		System.DateTime dtCurTime = System.DateTime.Now;
		System.DateTime dtEpochStartTime = System.Convert.ToDateTime("1/1/1970 0:00:00 AM");
		System.TimeSpan ts = dtCurTime.Subtract(dtEpochStartTime);

		long epochtime;
		epochtime = ((((((ts.Days * 24) + ts.Hours) * 60) + ts.Minutes) * 60) + ts.Seconds);

		return epochtime;
	}


}
