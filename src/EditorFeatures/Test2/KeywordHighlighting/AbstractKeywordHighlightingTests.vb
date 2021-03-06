' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Editor.Implementation.Highlighting
Imports Microsoft.CodeAnalysis.Editor.Shared.Extensions
Imports Microsoft.CodeAnalysis.Editor.Shared.Options
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
Imports Microsoft.CodeAnalysis.Shared.Extensions
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.VisualStudio.Text
Imports Roslyn.Test.Utilities
Imports Roslyn.Utilities

Namespace Microsoft.CodeAnalysis.Editor.UnitTests.KeywordHighlighting

    Public MustInherit Class AbstractKeywordHighlightingTests
        Protected Sub VerifyHighlights(test As XElement, Optional optionIsEnabled As Boolean = True)
            Using workspace = TestWorkspaceFactory.CreateWorkspace(test)
                Dim testDocument = workspace.Documents.Single(Function(d) d.CursorPosition.HasValue)
                Dim buffer = testDocument.TextBuffer
                Dim snapshot = testDocument.InitialTextSnapshot
                Dim caretPosition = testDocument.CursorPosition.Value
                Dim document As Document = workspace.CurrentSolution.Projects.First.Documents.First

                workspace.Options = workspace.Options.WithChangedOption(FeatureOnOffOptions.KeywordHighlighting, document.Project.Language, optionIsEnabled)

                Dim highlightingService = workspace.GetService(Of IHighlightingService)()
                Dim tagProducer = New HighlighterTagProducer(highlightingService)

                Dim producedTags = From tag In tagProducer.ProduceTagsAsync(document,
                                                                            New SnapshotSpan(snapshot, 0, snapshot.Length),
                                                                            New SnapshotPoint(snapshot, caretPosition),
                                                                            cancellationToken:=Nothing).Result
                                   Order By tag.Span.Start
                                   Select (tag.Span.Span.ToTextSpan().ToString())

                Dim expectedTags As New List(Of String)

                For Each hostDocument In workspace.Documents
                    For Each span In hostDocument.SelectedSpans
                        expectedTags.Add(span.ToString())
                    Next
                Next

                AssertEx.Equal(expectedTags, producedTags)
            End Using
        End Sub

    End Class

End Namespace
