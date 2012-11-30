﻿using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RedStapler.StandardLibrary;
using RedStapler.StandardLibrary.Collections;
using RedStapler.StandardLibrary.DataAccess;
using RedStapler.StandardLibrary.DataAccess.CommandWriting;
using RedStapler.StandardLibrary.DatabaseSpecification;
using RedStapler.StandardLibrary.DatabaseSpecification.Databases;
using RedStapler.StandardLibrary.InstallationSupportUtility.DatabaseAbstraction;

namespace EnterpriseWebLibrary.DevelopmentUtility.Operations.CodeGeneration.DataAccess {
	internal static class DataAccessStatics {
		// Matches spaced followed by @abc. The space prevents @@identity, etc. from getting matched.
		private const string sqlServerParamRegex = @"(?<!@)@\w*\w";

		private const string oracleParamRegex = @"(?<!:):\w*\w";

		/// <summary>
		/// Given a string, returns all instances of @abc in an ordered set
		/// containing abc (the token without the @ sign).  If a token is
		/// used more than once, it only appears in the list once.
		/// Does the same thing with :.
		/// </summary>
		internal static ListSet<string> GetNamedParamList( DatabaseInfo info, string statement ) {
			// We don't want to find parameters in quoted text.
			statement = statement.RemoveTextBetweenStrings( "'", "'" ).RemoveTextBetweenStrings( "\"", "\"" );

			MatchCollection matches;
			if( info is SqlServerInfo )
				matches = Regex.Matches( statement, sqlServerParamRegex );
			else
				matches = Regex.Matches( statement, oracleParamRegex );

			var parameters = new ListSet<string>();
			foreach( Match match in matches )
				parameters.Add( match.Value.Substring( 1 ) );

			return parameters;
		}

		/// <summary>
		/// Given raw query text such as that from Development.xml, returns a command that has had all of its parameters filled in with
		/// good dummy values and is ready to safely execute using schema only or key info behavior.
		/// </summary>
		internal static DbCommand GetCommandFromRawQueryText( DBConnection cn, string commandText ) {
			// This replacement is necessary because SQL Server chooses to care about the type of the parameter passed to TOP.
			commandText = Regex.Replace( commandText, @"TOP\( *@\w+ *\)", "TOP 0", RegexOptions.IgnoreCase );

			var cmd = cn.DatabaseInfo.CreateCommand();
			cmd.CommandText = commandText;
			foreach( var param in GetNamedParamList( cn.DatabaseInfo, cmd.CommandText ) )
				cmd.Parameters.Add( new DbCommandParameter( param, new DbParameterValue( "0" ) ).GetAdoDotNetParameter( cn.DatabaseInfo ) );
			return cmd;
		}

		internal static void WriteRowClass( TextWriter writer, IEnumerable<Column> columns, DatabaseInfo databaseInfo ) {
			CodeGenerationStatics.AddSummaryDocComment( writer, "Holds data for a row of this result." );
			writer.WriteLine( "public partial class Row: System.IEquatable<Row> {" );

			foreach( var column in columns )
				writer.WriteLine( "private readonly " + column.DataTypeName + " " + getMemberVariableName( column.Name ) + ";" );

			writer.WriteLine( "internal Row( DbDataReader reader ) {" );
			var cnt = 0;
			foreach( var column in columns ) {
				if( column.AllowsNull ) {
					writer.WriteLine( "if( reader.IsDBNull( " + cnt + " ) ) " + getMemberVariableName( column.Name ) + " = null;" );
					writer.WriteLine( "else" );
				}
				writer.WriteLine( "" + getMemberVariableName( column.Name ) + " = " + ( "(" + column.DataTypeName + ")" ) + "reader.GetValue( " + cnt + " );" );
				cnt++;
			}
			writer.WriteLine( "}" ); // constructor

			foreach( var column in columns ) {
				writeColumnProperty( writer, column, databaseInfo );
				if( column.HasNullabilityMismatch )
					writeTypedColumnProperty( writer, column );
			}

			// NOTE: Being smarter about the hash code could make searches of the collection faster.
			writer.WriteLine( "public override int GetHashCode() { " );
			// NOTE: Catch an exception generated by not having any uniquely identifying columns and rethrow it as a UserCorrectableException.
			writer.WriteLine( "return " + getMemberVariableName( columns.First( c => c.UseToUniquelyIdentifyRow ).Name ) + ".GetHashCode();" );
			writer.WriteLine( "}" ); // Object override of GetHashCode

			writer.WriteLine( @"	public static bool operator == (Row row1, Row row2 ) {
				return Equals( row1, row2 );
			}

			public static bool operator !=( Row row1, Row row2 ) {
				return !Equals( row1, row2 );
			}" );

			writer.WriteLine( "public override bool Equals( object obj ) {" );
			writer.WriteLine( "return Equals( obj as Row );" );
			writer.WriteLine( "}" ); // Object override of Equals

			writer.WriteLine( "public bool Equals( Row other ) {" );
			writer.WriteLine( "if( other == null ) return false;" );

			var condition = "";
			foreach( var column in columns.Where( ( c, index ) => c.UseToUniquelyIdentifyRow ) )
				condition = StringTools.ConcatenateWithDelimiter( " && ",
				                                                  condition,
				                                                  getMemberVariableName( column.Name ) + " == other." + getMemberVariableName( column.Name ) );
			writer.WriteLine( "return " + condition + ";" );
			writer.WriteLine( "}" ); // Equals method

			writer.WriteLine( "}" ); // class
		}

		private static void writeColumnProperty( TextWriter writer, Column column, DatabaseInfo databaseInfo ) {
			var isOracleClob = databaseInfo is OracleInfo && new[] { "Clob", "NClob" }.Contains( column.DbTypeString );
			CodeGenerationStatics.AddSummaryDocComment( writer, "This object will " + ( column.AllowsNull && !isOracleClob ? "sometimes" : "never" ) + " be null." );
			writer.WriteLine( "public " + column.DataTypeName + " " + column.Name + " { get { return " + getMemberVariableName( column.Name ) +
			                  ( isOracleClob ? " ?? \"\"" : "" ) + "; } }" );
		}

		private static void writeTypedColumnProperty( TextWriter writer, Column column ) {
			// NOTE: Delete this since we now use nullable types in the main properties.
			CodeGenerationStatics.AddSummaryDocComment( writer, "Do not call this property. It will be deleted." );
			writer.WriteLine( "public " + column.DataTypeIfNotNullName + " " + column.Name + "Typed { get { return (" + column.DataTypeIfNotNullName + ")" +
			                  getMemberVariableName( column.Name ) + "; } }" );
		}

		private static string getMemberVariableName( string columnName ) {
			return "_" + getCamelCaseName( columnName );
		}

		// NOTE: Move this to Standard Library.
		private static string getCamelCaseName( string val ) {
			return val.Substring( 0, 1 ).ToLower() + val.Substring( 1 );
		}

		internal static string GetMethodParamsFromCommandText( DatabaseInfo info, string commandText ) {
			var methodParams = "DBConnection cn";
			foreach( var param in GetNamedParamList( info, commandText ) )
				methodParams += ", " + "object " + param;
			return methodParams;
		}

		internal static void WriteAddParamBlockFromCommandText( TextWriter writer, string commandVariable, DatabaseInfo info, string commandText ) {
			foreach( var param in GetNamedParamList( info, commandText ) ) {
				writer.WriteLine( commandVariable + ".Parameters.Add( new DbCommandParameter( \"" + param + "\", new DbParameterValue( " + param +
				                  " ) ).GetAdoDotNetParameter( cn.DatabaseInfo ) );" );
			}
		}

		internal static bool IsRevisionHistoryTable( string table, RedStapler.StandardLibrary.Configuration.SystemDevelopment.Database configuration ) {
			return configuration.revisionHistoryTables != null &&
			       configuration.revisionHistoryTables.Any( revisionHistoryTable => revisionHistoryTable.EqualsIgnoreCase( table ) );
		}

		internal static string GetTableConditionInterfaceName( Database database, string table ) {
			return database.SecondaryDatabaseName + "CommandConditions." + table + "TableCondition";
		}

		internal static void WriteAddLatestRevisionsConditionMethod( TextWriter writer, string revisionIdColumn ) {
			writer.WriteLine( "private static void addLatestRevisionsCondition( InlineDbCommandWithConditions command ) {" );
			writer.WriteLine( "var revisionHistorySetup = AppTools.SystemLogic as RevisionHistorySetup;" );
			writer.WriteLine( "command.AddCondition( new InCondition( \"" + revisionIdColumn + "\", revisionHistorySetup.GetLatestRevisionsQuery() ) );" );
			writer.WriteLine( "}" );
		}
	}
}