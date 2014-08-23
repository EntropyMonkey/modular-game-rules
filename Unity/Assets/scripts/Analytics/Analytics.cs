using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Net;
using System;

public class Analytics : MonoBehaviour
{
	public static string CurrentUserId;
	public static int CurrentTask = 0;
	public static int LastTask = 0;

	public static bool Running
	{
		get
		{
			return localLog != null && localLog.Count > 0;
		}
	}

	// categories
	public const string gameEvent = "game";
	public const string testEvent = "test";
	public const string ruleEvent = "rule";
	public const string dataEvent = "data";
	public const string errorEvent = "error";

	// actions
	public const string send_mail = "send_mail";

	public const string load_rules = "load_rules";
	public const string save_rules = "save_rules";
	public const string delete_rules = "delete_rules";
	public const string load_level = "load_level";

	public const string add_event = "add_event";
	public const string add_reaction = "add_reaction";
	public const string add_actor = "add_actor";
	public const string change_param = "change_param";
	public const string change_name = "change_name";
	public const string delete_rule = "delete_rule";
	public const string delete_reaction = "delete_reaction";
	public const string delete_actor = "delete_actor";

	public const string test_game = "test_game";
	public const string edit_rules = "edit_rules";
	public const string discard_changes = "discard_changes";

	public const string start_test = "start_test";
	public const string end_test = "end_test";
	public const string start_task = "start_task";
	public const string end_task = "end_task";
	public const string task_duration = "task_duration";

	public const string exception_thrown = "exception_thrown";

	private static List<string> localLog;

	private static float time;
	private static float currentTaskTime;

	public static void StartSession(string userId)
	{
		if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
		{
			GoogleAnalyticsHelper.Settings("UA-53640697-1", "http://cookiecoffeecode.tumblr.com/");

			time = 0;

			CurrentUserId = userId;

			localLog = new List<string>();

			LogEvent(testEvent, start_test, "");
		}
	}

	void OnEnable()
	{
		Application.RegisterLogCallback(HandleLogEntry);
	}

	void OnDisable()
	{
		Application.RegisterLogCallback(null);
	}

	void HandleLogEntry(string logEntry, string stackTrace, LogType logType)
	{
		switch (logType)
		{
			case LogType.Exception:
				LogEvent(errorEvent, exception_thrown, logEntry);
				break;
		}
	}

	void Update()
	{
		if (localLog != null && Running)
			time += Time.deltaTime;
	}

	public static void StartTask(int number)
	{
		CurrentTask = number;

		currentTaskTime = Time.time;

		LogEvent(testEvent, start_task, number.ToString());
	}

	public static void EndTask()
	{
		LogEvent(testEvent, end_task, CurrentTask.ToString());
		LogEvent(testEvent, task_duration, (Time.time - currentTaskTime).ToString());
		LastTask = CurrentTask;
		CurrentTask = 0;
	}

	public static void LogEvent(string category, string action, string label)
	{
		if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
		{
			GoogleAnalyticsHelper.LogEvent(CurrentUserId, category, action, label, (int)time);
		}
		localLog.Add("" + CurrentUserId + " " + (int)time + " " + category + " " + action + " " + label);
	}

	void OnDestroy()
	{
		EndSession();
	}

	public static void EndSession()
	{
		if (!Running) return;

		LogEvent(testEvent, end_test, "");

		GoogleAnalyticsHelper.LogPage(CurrentUserId + "end");

		SaveToCSV(localLog, "log");

		localLog.Clear();

		System.Diagnostics.Process.Start(Application.dataPath + @"/Rules/");
		//SendMail(path, "UX result " + CurrentUserId);
	}

	static string SaveToCSV(List<string> table, string filename)
	{
		string dir = Application.dataPath + @"/Rules/";
		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}

		string filepath = dir + filename + ".csv";
		if (!File.Exists(filepath))
		{
			File.Create(filepath).Close();
		}
		string delimiter = ",";

		StringBuilder sb = new StringBuilder();
		for (int index = 0; index < table.Count; index++)
		{
			sb.AppendLine(string.Join(delimiter, table[index].Split(' ')));
		}

		File.AppendAllText(filepath, sb.ToString());

		return filepath;
	}

	// freezes unity editor!
	static void SendMail(string logfilePath, string subject, string recipient = MailConst.Username, string body = "")
	{
		SmtpClient smtpClient = new SmtpClient();
		NetworkCredential basicCredentials = new NetworkCredential(MailConst.Username, MailConst.Password);
		MailMessage message = new MailMessage();

		Debug.Log(1);
		Debug.Break();

		smtpClient.Host = MailConst.SmtpServer;
		smtpClient.UseDefaultCredentials = false;
		smtpClient.Credentials = basicCredentials as ICredentialsByHost;
		smtpClient.Timeout = 100;
		smtpClient.EnableSsl = true;
		smtpClient.Port = 465;

		Debug.Log(2);
		Debug.Break();

		message.From = new MailAddress(MailConst.Username);
		message.Subject = subject;
		message.IsBodyHtml = false;
		message.Body = body;
		message.To.Add(recipient);

		Debug.Log(3);
		Debug.Break();

		if (logfilePath != "")
		{
			message.Attachments.Add(new Attachment(logfilePath));
		}

		Debug.Log(4);
		Debug.Break();

		try
		{
			smtpClient.Send(message);
			LogEvent(testEvent, send_mail, "");
		}
		catch (Exception e)
		{
			LogEvent(errorEvent, send_mail, e.Message);
			Debug.LogError(e.InnerException + " " + e.Message);
		}
	}
}
