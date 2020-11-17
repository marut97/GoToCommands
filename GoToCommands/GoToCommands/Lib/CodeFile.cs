using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToCommands.Lib
{
	public static class CodeFile
	{
		public enum ProjectType
		{
			None,
			Code,
			Test
		};

		public enum FileType
		{
			Source,
			Header,
			Invalid
		}

		public enum FunctionType
		{
			Virtual,
			PureVirtual,
			Other
		}

		public static bool IsInterface { get; private set; }
		public static bool HasBaseClass { get; private set; }
		public static bool HasDerivedClass { get; private set; }
		public static String ClassName { get; private set; }
		public static ProjectType Project { get; private set; }
		public static FileType File { get; private set; }

		public static void set(Document document)
		{
			if (!reset(document))
				return;

			_time = DateTime.Now;
			_documentName = document.FullName;

			ClassName = "";

			setProjectType(document);
			setFileType(document.Name);
			setClassType(document);
		}

		private static void setClassType(Document document)
		{
			if (File != FileType.Header)
			{
				invalidClass();
				return;
			}
			if (document.Selection is TextSelection selection)
			{
				_lineNumber = selection.ActivePoint.Line;
				CodeClass classModel = getClass(document.ProjectItem.FileCodeModel.CodeElements);
				if (classModel == null)
				{
					invalidClass();
					return;
				}
				analyzeClass(classModel);
			}
			else
			{
				invalidClass();
			}
		}

		private static bool isPowerOfTwo(Int32 n)
		{
			if (n < 0)
				return false;

			return (Math.Ceiling((Math.Log(n) / Math.Log(2)))) == (Math.Floor(((Math.Log(n) / Math.Log(2)))));
		}

		private static FunctionType functionType(CodeFunction codeFunction)
		{
			var functionType = codeFunction.FunctionKind;

			if (isPowerOfTwo((Int32)functionType - (Int32)vsCMFunction.vsCMFunctionFunction - (Int32)vsCMFunction.vsCMFunctionVirtual - (Int32)vsCMFunction.vsCMFunctionPure) )
				return FunctionType.PureVirtual;

			if (isPowerOfTwo((Int32)functionType - (Int32)vsCMFunction.vsCMFunctionFunction - (Int32)vsCMFunction.vsCMFunctionVirtual) || (Int32)functionType == _overridenMethod)
				return FunctionType.Virtual;

			return FunctionType.Other;
		}

		private static void analyzeClass(CodeClass classModel)
		{
			ClassName = classModel.Name;
			HasBaseClass = classModel.Bases.Count > 0;
			bool hasPureVirtual = false;
			bool hasVirtual = false;
			foreach (var subElement in classModel.Children)
			{
				if (subElement is CodeFunction)
				{
					switch (functionType(subElement as CodeFunction))
					{
						case FunctionType.PureVirtual :
							hasPureVirtual = true;
							hasVirtual = true;
							break;
						case FunctionType.Virtual:
							hasVirtual = true;
							break;
						default:
							break;
					}

				}
				if (hasPureVirtual)
					break;
			}
			IsInterface = hasPureVirtual;
			HasDerivedClass = hasVirtual;
		}

		private static void invalidClass()
		{
			IsInterface = false;
			HasBaseClass = false;
			HasDerivedClass = false;
		}

		private static void setProjectType(Document document)
		{
			if (document.ProjectItem == null)
			{
				Project = ProjectType.None;
			}
			else
			{
				if (!Utilities.IsInVSProject(document.ProjectItem.ContainingProject.Name))
					Project = ProjectType.None;
				Project = Utilities.IsTestProject(document.ProjectItem.ContainingProject.Name) ? ProjectType.Test : ProjectType.Code;
			}
		}

		private static void setFileType(String name)
		{
			if (Utilities.IsHeader(name))
				File = FileType.Header;
			else if (Utilities.IsCode(name))
				File = FileType.Source;
			else
				File = FileType.Invalid;
		}

		private static bool reset(Document document)
		{
			if (document.Selection is TextSelection selection)
			{
				return !((DateTime.Now - _time < TimeSpan.FromSeconds(3)) && (document.FullName == _documentName) && (selection.ActivePoint.Line == _lineNumber));
			}
			return true;
		}

		private static CodeClass getClass(CodeElements elements)
		{
			foreach (CodeElement codeElement in elements)
			{
				if (codeElement is CodeClass codeClass)
				{
					if (codeClass.StartPoint.Line <= _lineNumber && codeClass.EndPoint.Line >= _lineNumber)
						return codeClass;
					continue;
				}
				if (codeElement is CodeNamespace)
				{
					var classModel = getClass(codeElement.Children);
					if (classModel != null)
						return classModel;
				}
			}
			return null;
		}

		private static DateTime _time;
		private static String _documentName;
		private static int _lineNumber;
		private static readonly int _overridenMethod = 2097280;
	}
}
