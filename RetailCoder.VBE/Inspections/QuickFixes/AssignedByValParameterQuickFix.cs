﻿using Rubberduck.Inspections.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using Rubberduck.VBEditor;
using Rubberduck.Inspections.Resources;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Symbols;
using System.Windows.Forms;
using Rubberduck.UI.Refactorings;

namespace Rubberduck.Inspections.QuickFixes
{
    public class AssignedByValParameterQuickFix : QuickFixBase
    {

        private Declaration _target;
        private string _localCopyVariableName;
        private bool _forceUseOfGeneratedName;
        private string[] _originalCodeLines;

        public AssignedByValParameterQuickFix(Declaration target, QualifiedSelection selection)
            : base(target.Context, selection, InspectionsUI.AssignedByValParameterQuickFix)
        {
            _target = target;
            _localCopyVariableName = "local" + CapitalizeFirstLetter(_target.IdentifierName);
            _forceUseOfGeneratedName = false;

            _originalCodeLines = GetMethodLines();
        }
        public override bool CanFixInModule { get { return false; } }
        public override bool CanFixInProject { get { return false; } }

        public override void Fix()
        {
            if (!_forceUseOfGeneratedName)
            {
                GetLocalCopyVariableNameFromUser();

                if (IsCancelled) { return; }
            }

            modifyBlockToUseLocalCopyVariable();

        }
        //This function created solely to support unit testing - prevents popup dialog 
        public void ForceFixUsingGeneratedName()
        {
            _forceUseOfGeneratedName = true;
            Fix();
        }

        private void GetLocalCopyVariableNameFromUser()
        {
            using (var view = new AssignedByValParameterQuickFixDialog(_originalCodeLines))
            {
                view.Target = _target;
                view.NewName = _localCopyVariableName;
                view.ShowDialog();

                IsCancelled = view.DialogResult == DialogResult.Cancel;
                if (!IsCancelled) { _localCopyVariableName = view.NewName; }
            }
        }
        private void modifyBlockToUseLocalCopyVariable()
        {
            if(ProposedNameIsInUse()) { return; }

            var module = Selection.QualifiedName.Component.CodeModule;
            int startLine = Selection.Selection.StartLine;

            module.InsertLines(++startLine, buildLocalCopyDeclaration());
            module.InsertLines(++startLine, buildLocalCopyAssignment());
            var moduleLines = GetModuleLines();
            //moduleLines array index is zero-based 
            string endOfScopeStatement = GetEndOfScopeStatementForDeclaration(moduleLines[Selection.Selection.StartLine - 1]);

            bool IsInScope = true;
            int obIndex; //One-Based index for module object
            int zbIndex; //Zero-Based index for moduleLines array
            //starts with lines after the above inserts
            for (zbIndex = startLine ; IsInScope && zbIndex < module.CountOfLines; zbIndex++)
            {
                obIndex = zbIndex + 1;
                if (LineRequiresUpdate(moduleLines[zbIndex]))
                {
                    string newStatement = moduleLines[zbIndex].Replace(_target.IdentifierName, _localCopyVariableName);
                    module.ReplaceLine(obIndex, newStatement);
                }
                IsInScope = !moduleLines[zbIndex].Contains(endOfScopeStatement);
            }
        }
        private bool ProposedNameIsInUse()
        {
            return GetMethodLines().Any(c => c.Contains(Tokens.Dim + " " + _localCopyVariableName + " "));
        }
        private bool LineRequiresUpdate(string line)
        {
            return line.Contains(" " + _target.IdentifierName + " ")
                                    || line.Contains(NameAsLeftHandSide())
                                    || line.Contains(NameAsRightHandSide())
                                    || line.Contains(NameAsObjectMethodOrAccessorCall())
                                    || line.Contains(NameAsSubOrFunctionParam())
                                    || line.Contains(NameAsSubOrFunctionParamFirst())
                                    || line.Contains(NameAsSubOrFunctionParamLast());
        }

        private string NameAsLeftHandSide() { return _target.IdentifierName + " "; }
        private string NameAsRightHandSide() { return " " + _target.IdentifierName; }
        private string NameAsObjectMethodOrAccessorCall() { return " " + _target.IdentifierName + "."; }
        private string NameAsSubOrFunctionParam() { return _target.IdentifierName + ","; }
        private string NameAsSubOrFunctionParamFirst() { return "(" + _target.IdentifierName; }
        private string NameAsSubOrFunctionParamLast() { return _target.IdentifierName + ")"; }


        private string buildLocalCopyDeclaration()
        {
            return Tokens.Dim + " " + _localCopyVariableName + " " + Tokens.As 
                + " " + _target.AsTypeName;
        }
        private string buildLocalCopyAssignment()
        {
            return (ValueTypes.Contains(_target.AsTypeName) ? string.Empty : Tokens.Set + " ") 
                + _localCopyVariableName + " = " + _target.IdentifierName; ;
        }
        private string[] GetModuleLines()
        {
            var module = Selection.QualifiedName.Component.CodeModule;
            var lines = module.GetLines(1, module.CountOfLines);
            string[] newLine = new string[] { "\r\n" };
            return lines.Split(newLine, StringSplitOptions.None);
        }
        private string[] GetMethodLines()
        {
            int zbIndex; //zero-based index
            zbIndex = Selection.Selection.StartLine - 1;
            //int startLine = Selection.Selection.StartLine;
            string[] allLines = GetModuleLines();

            string endStatement = GetEndOfScopeStatementForDeclaration(allLines[zbIndex]);

            bool IsInScope = true;
            System.Collections.Generic.List<string> codeBlockLines = new System.Collections.Generic.List<string>();
            for ( ; IsInScope && zbIndex < allLines.Count(); zbIndex++)
            {
                codeBlockLines.Add(allLines[zbIndex]);
                IsInScope = !allLines[zbIndex].Contains(endStatement);
            }
            return codeBlockLines.ToArray();
        }
        private string GetEndOfScopeStatementForDeclaration(string declaration)
        {
            return declaration.Contains("Sub ") ? "End Sub" : "End Function";
        }
        private static readonly IReadOnlyList<string> ValueTypes = new[]
        {
            Tokens.Boolean,
            Tokens.Byte,
            Tokens.Currency,
            Tokens.Date,
            Tokens.Decimal,
            Tokens.Double,
            Tokens.Integer,
            Tokens.Long,
            Tokens.LongLong,
            Tokens.Single,
            Tokens.String,
            Tokens.Variant
        };
        private string CapitalizeFirstLetter(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }

    }
}
