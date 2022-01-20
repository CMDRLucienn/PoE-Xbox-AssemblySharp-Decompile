// IEMod.Helpers.IEDebug
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Patchwork.Attributes;
using UnityEngine;

[PatchedByType("IEMod.Helpers.IEDebug")]
[NewType(null, null)]
public static class IEDebug
{
	private static readonly TextWriter _logger;

	private static readonly Stream _innerStream;

	[PatchedByMember("System.Void IEMod.Helpers.IEDebug::.cctor()")]
	static IEDebug()
	{
		FileStream fileStream = (FileStream)(_innerStream = File.Open("IEMod.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
		StreamWriter streamWriter = (StreamWriter)(_logger = new StreamWriter(_innerStream));
	}

	[PatchedByMember("System.Void IEMod.Helpers.IEDebug::Log(System.Object)")]
	public static void Log(object format)
	{
		if (format != null && (!(format is string) || !string.IsNullOrEmpty((string)format)))
		{
			_logger.WriteLine(format);
			_logger.Flush();
			_innerStream.Flush();
		}
	}

	[PatchedByMember("System.Void IEMod.Helpers.IEDebug::Log(System.String,System.Object[])")]
	public static void Log(string format, params object[] args)
	{
		Log((object)string.Format(format, args));
	}

	[PatchedByMember("IEMod.Helpers.IEModException IEMod.Helpers.IEDebug::Exception(System.Exception,System.String,System.Object[])")]
	public static IEModException Exception(Exception innerEx, string message, params object[] args)
	{
		Log("!! EXCEPTION !!: " + message, args);
		args = args ?? new object[0];
		IndentedTextWriter writer = new IndentedTextWriter(new StringWriter());
		PrintStackTrace(writer, new StackTrace(1));
		return new IEModException(string.Format(message, args), innerEx);
	}

	[PatchedByMember("System.Void IEMod.Helpers.IEDebug::PrintStackTrace(System.CodeDom.Compiler.IndentedTextWriter,System.Diagnostics.StackTrace)")]
	private static void PrintStackTrace(IndentedTextWriter writer, StackTrace trace)
	{
		StackFrame[] frames = trace.GetFrames();
		if (frames == null)
		{
			writer.WriteLine("(none)");
			return;
		}
		for (int i = 0; i < frames.Length; i++)
		{
			StackFrame stackFrame = frames[i];
			writer.WriteLine("{0}. At {1}", i, stackFrame.GetMethod());
			writer.Indent++;
			writer.WriteLine("Source Location: {0}, ln# {1}, col# {2}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber(), stackFrame.GetFileColumnNumber());
			writer.WriteLine("IL Offset: {0}, Native Offset: {1}", stackFrame.GetILOffset(), stackFrame.GetNativeOffset());
			writer.Indent--;
		}
	}

	[PatchedByMember("System.Void IEMod.Helpers.IEDebug::PrintExceptionWithoutTrace(System.CodeDom.Compiler.IndentedTextWriter,System.Exception)")]
	private static void PrintExceptionWithoutTrace(IndentedTextWriter iWriter, Exception ex)
	{
		if (ex == null)
		{
			iWriter.WriteLine("(null)");
			return;
		}
		iWriter.WriteLine("[{0}]", ex.GetType());
		iWriter.Indent++;
		iWriter.WriteLine("Message: {0}", ex.Message);
		iWriter.WriteLine("Source: {0}", ex.Source);
		iWriter.WriteLine("TargetSite: {0}", ex.TargetSite);
		iWriter.WriteLine("HelpLink: {0}", ex.HelpLink);
		if (ex.Data.Count > 0)
		{
			iWriter.WriteLine("Data:");
			iWriter.Indent++;
			foreach (object key in ex.Data.Keys)
			{
				iWriter.WriteLine("• {0} = {1}", key, ex.Data[key]);
			}
			iWriter.Indent--;
		}
		iWriter.WriteLine("Inner Exception: ");
		iWriter.Indent++;
		PrintExceptionWithoutTrace(iWriter, ex.InnerException);
		iWriter.Indent--;
	}

	[PatchedByMember("System.String IEMod.Helpers.IEDebug::PrintException(System.Exception)")]
	public static string PrintException(Exception ex)
	{
		StringWriter stringWriter = new StringWriter();
		IndentedTextWriter indentedTextWriter = new IndentedTextWriter(stringWriter);
		PrintExceptionWithoutTrace(indentedTextWriter, ex);
		indentedTextWriter.WriteLine("Unity Stack Trace:");
		indentedTextWriter.Indent++;
		string[] source = StackTraceUtility.ExtractStringFromException(ex).Split(new string[2] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		source.ToList().ForEach(indentedTextWriter.WriteLine);
		indentedTextWriter.Flush();
		stringWriter.Flush();
		return stringWriter.ToString();
	}
}
