Imports System.Windows.Forms

Public Class FormProperties
    Private _mydcam As MyDcam

    ' ----------------

    Class dataprop
        Private _dcamprop As MyDcamProp
        Private _propname As String
        Private _propvalue As Double
        Private _dicttextvalue As Dictionary(Of String, Double)
        Private _dictvaluetext As Dictionary(Of Double, String)

        Public Sub New(ByVal myprop As MyDcamProp)
            _dcamprop = myprop
            _propname = _dcamprop.getname()
            _dcamprop.getvalue(_propvalue)
        End Sub
        Public ReadOnly Property name() As String
            Get
                Return _propname
            End Get
        End Property

        Public Property value() As String
            Get
                If _dictvaluetext Is Nothing Then
                    Return Str(_propvalue)
                ElseIf Not _dictvaluetext.ContainsKey(_propvalue) Then
                    ' this should not happen
                    Return Str(_propvalue)
                Else
                    Return _dictvaluetext(_propvalue)
                End If
            End Get
            Set(ByVal text As String)
                If _dicttextvalue Is Nothing Then
                    _propvalue = Val(text)
                ElseIf Not _dicttextvalue.ContainsKey(text) Then
                    _propvalue = Val(text)
                Else
                    _propvalue = _dicttextvalue(text)
                End If
                _dcamprop.setgetvalue(_propvalue)
            End Set
        End Property

        Public Function is_readonly() As Boolean
            Return _dcamprop.is_attr_readonly()
        End Function

        Public Function get_cell() As DataGridViewCell
            Dim cell As DataGridViewCell

            _dcamprop.update_attr()

            If _dcamprop.is_attrtype_mode() Then
                _dicttextvalue = New Dictionary(Of String, Double)
                _dictvaluetext = New Dictionary(Of Double, String)


                Dim cbcell As New DataGridViewComboBoxCell
                Dim value As Double
                value = _dcamprop.m_attr.valuemin

                While value <= _dcamprop.m_attr.valuemax
                    Dim text As String
                    text = _dcamprop.getvaluetext(value)
                    _dicttextvalue.Add(text, value)
                    _dictvaluetext.Add(value, text)
                    cbcell.Items.Add(text)
                    If Not _dcamprop.queryvalue_next(value) Then
                        Exit While
                    End If
                End While

                cbcell.Value = _dictvaluetext(_propvalue)
                '    cbccell.ReadOnly = False

                cell = cbcell
            Else
                _dicttextvalue = Nothing
                _dictvaluetext = Nothing

                Dim tbcell As New DataGridViewTextBoxCell
                cell = tbcell
            End If

            Return cell
        End Function
    End Class

    ' ----------------

    Public Sub set_mydcam(ByRef mydcam As MyDcam)
        ' update DataSource of DataGridView
        Dim listprop As New List(Of dataprop)

        Dim myprop As New MyDcamProp(mydcam, 0)
        While myprop.nextid()
            listprop.Add(New dataprop(myprop.Clone()))
        End While
        DataGridViewProp.DataSource = listprop
    End Sub

    Public Sub update_properties()
        ' update cells in DataGridView
        DataGridViewProp.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

        For Each row As DataGridViewRow In DataGridViewProp.Rows()
            Dim i As Integer
            i = row.Index()

            Dim aPropdata As dataprop = TryCast(row.DataBoundItem, dataprop)
            If aPropdata IsNot Nothing Then
                Dim cell As DataGridViewCell
                cell = aPropdata.get_cell()
                DataGridViewProp.Item(1, i) = cell
                If aPropdata.is_readonly() Then
                    cell.ReadOnly = True        ' Readonly can change only after attached DataGridView
                End If
            End If
        Next
    End Sub

    Private Sub FrmProperties_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' set DataGridViewProp style
        DataGridViewProp.AllowUserToAddRows = False
        DataGridViewProp.AllowUserToDeleteRows = False
        DataGridViewProp.RowHeadersVisible = False
    End Sub
End Class
