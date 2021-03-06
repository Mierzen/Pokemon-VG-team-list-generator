﻿Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports iTextSharp.text.pdf

Public Class Form1
    Dim age As Integer

#Region "Validation"
    Private Sub tb_name_Leave(sender As Object, e As EventArgs) Handles tb_name.Leave
        Dim str As String = tb_name.Text

        For i = 0 To str.Length - 1
            If Char.IsLetter(str(i)) OrElse str(i) = "-" Then
            Else
                MsgBox("Please enter only alphabetic characters.", vbOKOnly Or vbCritical, "Incorrect name")
                tb_name.Focus()
                tb_name.SelectAll()
            End If
        Next
    End Sub

    Private Sub tb_playerID_Leave(sender As Object, e As EventArgs) Handles tb_playerID.Leave
        Dim isInt As Boolean = Integer.TryParse(tb_playerID.Text, isInt)

        If Strings.Len(tb_playerID.Text) <> 7 OrElse isInt = False Then
            MsgBox("Incorrect Player ID entered." & vbNewLine & "Please correct your Player ID.", vbOKOnly Or vbCritical, "Incorrect Player ID")
            tb_playerID.Focus()
            tb_playerID.SelectAll()
        End If
    End Sub

    Private Sub dtp_dateOfBirth_Leave(sender As Object, e As EventArgs) Handles dtp_dateOfBirth.Leave
        age = Date.Today.Year - dtp_dateOfBirth.Value.Year
        If age < 5 Then
            MsgBox("Please check your date of birth.", vbOKOnly Or vbCritical, "Incorrect player age")
        End If
    End Sub
#End Region

    Private Sub btn_createForm_Click(sender As Object, e As EventArgs) Handles btn_createForm.Click
        If tb_name.Text = "" OrElse tb_playerID.Text = "" OrElse dtp_dateOfBirth.Value.Date = Date.Today Then
            MsgBox("Form input not complete." & vbNewLine & "Please fill in the needed text boxes.", vbOKOnly Or vbCritical, "Incomplete form entry")
            Exit Sub
        End If

        Dim team As New List(Of Pokemon)
        Dim temp As New List(Of String)

#Region "Check line count"
        Dim lineCount As Integer = 0
        Dim pokCount As Integer = 0

        For Each line In tb_teamList.Lines
            If line = "" OrElse line = Nothing Then
                'do nothing
            Else
                lineCount += 1

                If InStr(line, "@") <> 0 Then
                    pokCount += 1
                End If
            End If
        Next

        If pokCount <> 6 Then
            MsgBox("It seems that there are not 6 Pokémon in your team." & vbNewLine & "Please check your input.", vbOKOnly Or vbCritical, "Invalid input")
            Exit Sub
        End If

        If lineCount = 49 OrElse lineCount = 49 - 6 Then 'EV lines may be omitted
            'it is right
        Else
            MsgBox("It seems that there is something wrong with your team list input." & vbNewLine & "Please check your input.", vbOKOnly Or vbCritical, "Invalid input")
            Exit Sub
        End If
#End Region


#Region "Read lines"
        For Each line In tb_teamList.Lines
            If line = "" OrElse line = Nothing Then
                'skip
            Else
                Dim loc As Integer

                loc = InStr(line, " @ ")
                If loc <> 0 Then 'new pokemon
                    temp.Add(Strings.Left(line, loc))
                    temp.Add(Strings.Right(line, Len(line) - Len(temp(0)) - Len(" @ ") + 1))
                End If

                loc = InStr(line, "Ability: ")
                If loc <> 0 Then
                    temp.Add(Strings.Right(line, Len(line) - Len("Ability: ")))
                End If

                loc = InStr(line, " Nature")
                If loc <> 0 Then
                    temp.Add(Strings.Left(line, Len(line) - Len(" Nature")))
                End If

                loc = InStr(line, "- ")
                Dim move As New List(Of String)
                If loc <> 0 Then
                    temp.Add(Strings.Right(line, Len(line) - Len("- ")))
                End If

                If temp.Count = 8 Then
                    team.Add(New Pokemon(temp))
                    temp.Clear()
                End If
            End If
        Next
#End Region

        Dim svd_saveFile As New SaveFileDialog
        With svd_saveFile
            .Title = "Save completed form"
            .DefaultExt = ".pdf"
            .Filter = "PDF files|*.pdf|All files|*.*"
        End With

        svd_saveFile.ShowDialog()

        If svd_saveFile.FileName = "" Then
            Exit Sub
        End If

        Dim pdfTemplate = My.Resources._2014_VG_XY_Team_List___Editable_Form
        Dim newFile As String = svd_saveFile.FileName

        Dim pdfReader As New PdfReader(pdfTemplate)
        Dim pdfStamper As New PdfStamper(pdfReader, New FileStream(newFile, FileMode.Create))
        Dim pdfFormFields As AcroFields = pdfStamper.AcroFields

#Region "Write PDF"
        pdfFormFields.SetField("Name", tb_name.Text)
        pdfFormFields.SetField("Player ID", tb_playerID.Text)
        pdfFormFields.SetField("Event Name", tb_eventName.Text)
        pdfFormFields.SetField("Age Division", If(age <= 11, "Junior", If(age <= 15, "Senior", "Master")))
        pdfFormFields.SetField("DOB", dtp_dateOfBirth.Value.Date)

        Dim i As Integer = 1
        For Each pokemon In team
            Dim suffix As String
            If i = 1 Then
                suffix = ""
            Else
                suffix = "_" & i
            End If

            pdfFormFields.SetField("Pokémon" & suffix, pokemon.Name)
            pdfFormFields.SetField("Held Item" & suffix, pokemon.Item)
            pdfFormFields.SetField("Ability" & suffix, pokemon.Ability)
            pdfFormFields.SetField("Nature" & suffix, pokemon.Nature)

            For j = 1 To 4
                pdfFormFields.SetField("Move " & j & suffix, pokemon.Moves(j - 1))
            Next

            i += 1
        Next

        pdfStamper.FormFlattening = False

        pdfStamper.Close()
#End Region

        Beep()

        Try
            Dim result As MsgBoxResult = MsgBox("Done!" & vbNewLine & vbNewLine & "Open the PDF file?", vbYesNo, "Open PDF?")

            If result = vbYes Then
                Dim p As New System.Diagnostics.Process
                Dim s As New System.Diagnostics.ProcessStartInfo(newFile)
                s.UseShellExecute = True
                s.WindowStyle = ProcessWindowStyle.Normal
                p.StartInfo = s
                p.Start()
            Else
                Dim result2 As MsgBoxResult = MsgBox("Open the file location?", vbYesNo, "Open folder?")
                If result2 = vbYes Then
                    Dim p As New System.Diagnostics.Process
                    Dim s As New System.Diagnostics.ProcessStartInfo(Strings.Left(newFile, InStrRev(newFile, "\")))
                    s.UseShellExecute = True
                    s.WindowStyle = ProcessWindowStyle.Normal
                    p.StartInfo = s
                    p.Start()
                End If
            End If
        Catch
        End Try


    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Select()
        dtp_dateOfBirth.Value = Date.Today
    End Sub

    Dim heightRequiredByRest As Integer
    Private Sub Form1_SizeChanged(sender As Object, e As EventArgs) Handles MyBase.SizeChanged
        If heightRequiredByRest = Nothing Then
            heightRequiredByRest = tlp_form.Size.Height - tb_teamList.Size.Height
        End If

        If Not heightRequiredByRest = Nothing Then
            tb_teamList.Size = New Size(tb_teamList.Width, tlp_form.Size.Height - heightRequiredByRest)
        End If

        btn_createForm.Margin = New Padding((Me.Size.Width - btn_createForm.Size.Width - 6 - 20) / 2, btn_createForm.Margin.Top, btn_createForm.Margin.Right, btn_createForm.Margin.Bottom)


        Dim availableWidth = CalcAvaliableCaptionWidth()
        Dim formTextWidth As Integer = TextRenderer.MeasureText("Pokémon VG team list generator", SystemFonts.CaptionFont).Width

        If formTextWidth > availableWidth Then
            Me.Text = "VG team list generator"
        Else
            Me.Text = "Pokémon VG team list generator"
        End If
    End Sub

    Private Function CalcAvaliableCaptionWidth() As Integer
        ' Form width
        ' Icon
        ' Minimize button (26 on Win8)
        ' Maximize button (26 on Win8)
        ' Close button (45 on Win8)
        Return Width - (If(Icon Is Nothing, 0, Icon.Width)) - (If(MinimizeBox, SystemInformation.CaptionButtonSize.Width, 0)) - SystemInformation.CaptionButtonSize.Width - SystemInformation.CaptionButtonSize.Width
    End Function
End Class

Public Class Pokemon
    Public Name As String
    Public Item As String
    Public Ability As String
    Public Nature As String
    Public Moves As List(Of String)

    Public Sub New(str As List(Of String))
        For Each Item In str
            Name = str(0)
            Item = str(1)
            Ability = str(2)
            Nature = str(3)

            Moves = New List(Of String)
            Moves.Add(str(4))
            Moves.Add(str(5))
            Moves.Add(str(6))
            Moves.Add(str(7))
        Next
    End Sub
End Class
