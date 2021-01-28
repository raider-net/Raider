using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Raider.Generator
{
	public class GeneratorBase
	{
		public enum WriteModes
		{
			None,
			Overwritte,
			Append
		}

		private class Block
		{
			public string? OutputPath { get; set; }
			public string? Name { get; set; }
			public int Start { get; set; }
			public int Length { get; set; }
			public bool IncludeInDefault { get; set; }
		}

		private StringBuilder? _generationEnvironment;
		private List<int>? _indentLengths;
		private string _currentIndent = "";
		private bool _endsWithNewline;
		private IDictionary<string, object>? _session;

		private ToStringInstanceHelper _toStringHelper = new ToStringInstanceHelper();

		private Block? _currentBlock;
		private List<Block> _blocks = new List<Block>();
		private Block? _footer = new Block();
		private Block? _header = new Block();
		private StringBuilder? _template;
		private bool? _generateUFT8WithBOM;

		public Dictionary<string, string> GeneratedFiles { get; }
		public WriteModes WriteMode { get; set; } = WriteModes.None;

		public List<GeneratorError> Errors { get; }

		public string? ErrorString()
		{
			if (Errors.Count == 0)
				return null;

			StringBuilder sb = new StringBuilder();
			foreach (var error in Errors)
			{
				string type = error.IsWarning ? "Warn" : "Error";
				sb.AppendLine($"{type}: {error.ErrorText} File={error.FileName}");
			}
			return sb.ToString();
		}

		public GeneratorBase()
		{
			Errors = new List<GeneratorError>();
			GeneratedFiles = new Dictionary<string, string>();
			Initialize();
		}

		public virtual string TransformText()
		{
			throw new NotImplementedException();
		}

		protected virtual void Initialize()
		{
		}

		protected void SetGenerationEnvironment(StringBuilder template, bool generateUFT8WithBOM = true)
		{
			this._template = template;
			this._generateUFT8WithBOM = generateUFT8WithBOM;
		}

		protected void WriteGlobalFooter()
		{
			CurrentBlock = _footer;
			_footer.IncludeInDefault = true;
		}

		protected void WriteGlobalHeader()
		{
			CurrentBlock = _header;
			_header.IncludeInDefault = true;
		}

		protected void StartNewFile(string outputPath, string name)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			CurrentBlock = new Block { Name = name, OutputPath = outputPath };
		}

		protected void SetSubDirectoriesPathToCurrentBlock(string subDirPath)
		{
			if (string.IsNullOrWhiteSpace(subDirPath))
			{
				return;
			}
			var dir = Path.GetDirectoryName(CurrentBlock.Name);
			var file = Path.GetFileName(CurrentBlock.Name);
			CurrentBlock.Name = Path.Combine(dir, subDirPath, file);
		}

		private void EndBlock()
		{
			if (CurrentBlock == null)
				return;
			CurrentBlock.Length = _template.Length - CurrentBlock.Start;
			if (CurrentBlock != _header && CurrentBlock != _footer)
				_blocks.Add(CurrentBlock);
			_currentBlock = null;
		}

		protected string GetCurrentContent()
		{
			return _template.ToString();
		}

		protected string GetCurrentBlockContent()
		{
			string headerText = _template.ToString(_header.Start, _header.Length);
			string footerText = _template.ToString(_footer.Start, _footer.Length);

			//if (!footer.IncludeInDefault)
			//    template.Remove(footer.Start, footer.Length);

			if (_blocks.Count == 0)
			{
				return GetCurrentContent();
			}
			Block lastBlock = _blocks[_blocks.Count - 1];
			return headerText + _template.ToString(lastBlock.Start, lastBlock.Length) + footerText;
		}

		public virtual void Process()
		{
			EndBlock();
			string headerText = _template.ToString(_header.Start, _header.Length);
			string footerText = _template.ToString(_footer.Start, _footer.Length);
			_blocks.Reverse();
			if (!_footer.IncludeInDefault)
				_template.Remove(_footer.Start, _footer.Length);
			foreach (Block block in _blocks)
			{
				string content = headerText + _template.ToString(block.Start, block.Length) + footerText;

				string fileName = block.Name;

				if (!string.IsNullOrWhiteSpace(block.OutputPath) || !string.IsNullOrWhiteSpace(fileName))
				{
					string filePath = Path.Combine(block.OutputPath, fileName);
					GeneratedFiles[filePath] = content;
					CreateFile(filePath, content);
				}

				_template.Remove(block.Start, block.Length);
			}
			if (!_header.IncludeInDefault)
				_template.Remove(_header.Start, _header.Length);
		}

		protected void NewProcess(List<string> initialFileNames)
		{
			_currentBlock = null;
			_blocks = new List<Block>();
			_footer = new Block();
			_header = new Block();
			_template = new StringBuilder();
			//GeneratedFiles = initialFileNames ?? new List<string>();
		}

		private void CreateFile(string filePath, string content)
		{
			if (WriteMode != WriteModes.None)
			{
				var directory = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				//if (IsFileContentDifferent(filePath, content))
				if (WriteMode == WriteModes.Overwritte)
					File.WriteAllText(filePath, content, _generateUFT8WithBOM == true ? Encoding.UTF8 : new UTF8Encoding(false));
				else if (WriteMode == WriteModes.Append)
					File.AppendAllText(filePath, content, _generateUFT8WithBOM == true ? Encoding.UTF8 : new UTF8Encoding(false));
			}
		}

		protected bool IsFileContentDifferent(string fileName, string newContent)
		{
			return !(File.Exists(fileName) && File.ReadAllText(fileName, _generateUFT8WithBOM == true ? Encoding.UTF8 : new UTF8Encoding(false)) == newContent);
		}

		private Block? CurrentBlock
		{
			get { return _currentBlock; }
			set
			{
				if (CurrentBlock != null)
					EndBlock();
				if (value != null)
					value.Start = _template.Length;
				_currentBlock = value;
			}
		}

		/// <summary>
		/// The string builder that generation-time code is using to assemble generated output
		/// </summary>
		protected StringBuilder GenerationEnvironment
		{
			get
			{
				if ((this._generationEnvironment == null))
				{
					this._generationEnvironment = new StringBuilder();
				}
				return this._generationEnvironment;
			}
			set
			{
				this._generationEnvironment = value;
			}
		}

		/// <summary>
		/// A list of the lengths of each indent that was added with PushIndent
		/// </summary>
		private List<int> IndentLengths
		{
			get
			{
				if ((this._indentLengths == null))
				{
					this._indentLengths = new List<int>();
				}
				return this._indentLengths;
			}
		}

		/// <summary>
		/// Gets the current indent we use when adding lines to the output
		/// </summary>
		protected string CurrentIndent
		{
			get
			{
				return this._currentIndent;
			}
		}

		/// <summary>
		/// Current transformation session
		/// </summary>
		protected virtual IDictionary<string, object> Session
		{
			get
			{
				return this._session;
			}
			set
			{
				this._session = value;
			}
		}

		protected bool ParamExists(string key)
		{
			return Session?.ContainsKey(key) ?? false;
		}

		internal void SetParamAndValue(string key, object value)
		{
			if (_session == null)
				_session = new Dictionary<string, object>();

			Session[key] = value;
		}

		protected string GetParam(string key)
		{
			return Session?[key]?.ToString();
		}

		public T GetParam<T>(string key)
		{
			return (T)Session?[key];
		}

		#region Transform-time helpers

		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		protected void Write(string textToAppend)
		{
			if (string.IsNullOrEmpty(textToAppend))
			{
				return;
			}
			// If we're starting off, or if the previous text ended with a newline,
			// we have to append the current indent first.
			if (((this.GenerationEnvironment.Length == 0)
						|| this._endsWithNewline))
			{
				this.GenerationEnvironment.Append(this._currentIndent);
				this._endsWithNewline = false;
			}
			// Check if the current text ends with a newline
			if (textToAppend.EndsWith(Environment.NewLine, StringComparison.CurrentCulture))
			{
				this._endsWithNewline = true;
			}
			// This is an optimization. If the current indent is "", then we don't have to do any
			// of the more complex stuff further down.
			if ((this._currentIndent.Length == 0))
			{
				this.GenerationEnvironment.Append(textToAppend);
				return;
			}
			// Everywhere there is a newline in the text, add an indent after it
			textToAppend = textToAppend.Replace(Environment.NewLine, (Environment.NewLine + this._currentIndent));
			// If the text ends with a newline, then we should strip off the indent added at the very end
			// because the appropriate indent will be added when the next time Write() is called
			if (this._endsWithNewline)
			{
				this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this._currentIndent.Length));
			}
			else
			{
				this.GenerationEnvironment.Append(textToAppend);
			}
		}

		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		protected void WriteLine(string textToAppend)
		{
			this.Write(textToAppend);
			this.GenerationEnvironment.AppendLine();
			this._endsWithNewline = true;
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		protected void Write(string format, params object[] args)
		{
			this.Write(string.Format(CultureInfo.CurrentCulture, format, args));
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		protected void WriteLine(string format, params object[] args)
		{
			this.WriteLine(string.Format(CultureInfo.CurrentCulture, format, args));
		}

		/// <summary>
		/// Increase the indent
		/// </summary>
		protected void PushIndent(string indent)
		{
			if ((indent == null))
			{
				throw new ArgumentNullException("indent");
			}
			this._currentIndent = (this._currentIndent + indent);
			this.IndentLengths.Add(indent.Length);
		}

		/// <summary>
		/// Remove the last indent that was added with PushIndent
		/// </summary>
		protected string PopIndent()
		{
			string returnValue = "";
			if ((this.IndentLengths.Count > 0))
			{
				int indentLength = this.IndentLengths[(this.IndentLengths.Count - 1)];
				this.IndentLengths.RemoveAt((this.IndentLengths.Count - 1));
				if ((indentLength > 0))
				{
					returnValue = this._currentIndent.Substring((this._currentIndent.Length - indentLength));
					this._currentIndent = this._currentIndent.Remove((this._currentIndent.Length - indentLength));
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Remove any indentation
		/// </summary>
		protected void ClearIndent()
		{
			this.IndentLengths.Clear();
			this._currentIndent = "";
		}

		#endregion Transform-time helpers

		#region ToString Helpers

		/// <summary>
		/// Utility class to produce culture-oriented representation of an object as a string.
		/// </summary>
		protected class ToStringInstanceHelper
		{
			private IFormatProvider _formatProvider = CultureInfo.InvariantCulture;

			/// <summary>
			/// Gets or sets format provider to be used by ToStringWithCulture method.
			/// </summary>
			public IFormatProvider FormatProvider
			{
				get
				{
					return this._formatProvider;
				}
				set
				{
					if ((value != null))
					{
						this._formatProvider = value;
					}
				}
			}

			/// <summary>
			/// This is called from the compile/run appdomain to convert objects within an expression block to a string
			/// </summary>
			public string ToStringWithCulture(object objectToConvert)
			{
				if ((objectToConvert == null))
				{
					throw new ArgumentNullException("objectToConvert");
				}
				Type t = objectToConvert.GetType();
				MethodInfo method = t.GetMethod("ToString", new Type[] {
							typeof(IFormatProvider)});
				if ((method == null))
				{
					return objectToConvert.ToString();
				}
				else
				{
					return ((string)(method.Invoke(objectToConvert, new object[] {
								this._formatProvider })));
				}
			}
		}

		/// <summary>
		/// Helper to produce culture-oriented representation of an object as a string
		/// </summary>
		protected ToStringInstanceHelper ToStringHelper
		{
			get
			{
				return this._toStringHelper;
			}
		}

		#endregion Helpers
	}

	public static class GeneratorBaseExtensions
	{
		public static T SetParam<T>(this T generator, string key, object value)
			where T : GeneratorBase
		{
			generator.SetParamAndValue(key, value);
			return generator;
		}

		public static T AddError<T>(this T generator, string message)
			where T : GeneratorBase
		{
			generator.Errors.Add(new GeneratorError
			{
				ErrorText = message,
				IsWarning = false
			});
			return generator;
		}

		public static T AddWarning<T>(this T generator, string message)
			where T : GeneratorBase
		{
			generator.Errors.Add(new GeneratorError
			{
				ErrorText = message,
				IsWarning = true
			});
			return generator;
		}

		public static T AddError<T>(this T generator, string fileName, string text)
			where T : GeneratorBase
		{
			generator.Errors.Add(new GeneratorError
			{
				FileName = fileName,
				ErrorText = text,
				IsWarning = false
			});
			return generator;
		}

		public static T AddWarning<T>(this T generator, string fileName, string text)
			where T : GeneratorBase
		{
			generator.Errors.Add(new GeneratorError
			{
				FileName = fileName,
				ErrorText = text,
				IsWarning = true
			});
			return generator;
		}
	}
}
