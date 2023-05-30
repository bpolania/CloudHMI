object fmWebServ: TfmWebServ
  Left = 894
  Top = 197
  Width = 592
  Height = 547
  Caption = 'fmWebServ'
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object lbOutroIHM: TLabel
    Left = 176
    Top = 8
    Width = 54
    Height = 13
    Caption = 'lbOutroIHM'
  end
  object lbReset: TLabel
    Left = 480
    Top = 56
    Width = 6
    Height = 13
    Caption = '0'
  end
  object Label1: TLabel
    Left = 480
    Top = 40
    Width = 61
    Height = 13
    Caption = 'Auto Resets:'
  end
  object btSinc: TButton
    Left = 488
    Top = 8
    Width = 65
    Height = 25
    Caption = 'Get Sync.'
    TabOrder = 0
    OnClick = btSincClick
  end
  object mmWeb: TMemo
    Left = 8
    Top = 72
    Width = 561
    Height = 433
    Anchors = [akLeft, akTop, akRight, akBottom]
    ScrollBars = ssVertical
    TabOrder = 1
  end
  object cbLogar: TCheckBox
    Left = 8
    Top = 48
    Width = 97
    Height = 17
    Caption = 'Log'
    TabOrder = 2
  end
  object IdHTTPServer1: TIdHTTPServer
    Bindings = <>
    CommandHandlers = <>
    DefaultPort = 51908
    Greeting.NumericCode = 0
    MaxConnectionReply.NumericCode = 0
    OnException = IdHTTPServer1Exception
    ReplyExceptionCode = 0
    ReplyTexts = <>
    ReplyUnknownCommand.NumericCode = 0
    Left = 336
    Top = 16
  end
  object NMHTTP1: TNMHTTP
    Port = 51908
    TimeOut = 1
    ReportLevel = 0
    OnStatus = NMHTTP1Status
    Body = 'Default.htm'
    Header = 'Head.txt'
    InputFileMode = False
    OutputFileMode = False
    OnSuccess = NMHTTP1Success
    ProxyPort = 0
    Left = 416
    Top = 48
  end
  object Timer2: TTimer
    Interval = 10000
    OnTimer = Timer2Timer
    Left = 368
    Top = 48
  end
  object Timer3: TTimer
    OnTimer = Timer3Timer
    Left = 368
    Top = 80
  end
  object IdHTTP1: TIdHTTP
    MaxLineAction = maException
    AllowCookies = True
    ProxyParams.BasicAuthentication = False
    ProxyParams.ProxyPort = 0
    Request.ContentLength = -1
    Request.ContentRangeEnd = 0
    Request.ContentRangeStart = 0
    Request.Accept = 'text/html, */*'
    Request.BasicAuthentication = False
    Request.UserAgent = 'Mozilla/3.0 (compatible; Indy Library)'
    HTTPOptions = [hoForceEncodeParams]
    Left = 416
    Top = 16
  end
  object NMHTTP2: TNMHTTP
    Port = 0
    TimeOut = 1
    ReportLevel = 0
    OnStatus = NMHTTP2Status
    Body = 'Default.htm'
    Header = 'Head.txt'
    InputFileMode = False
    OutputFileMode = False
    OnSuccess = NMHTTP2Success
    ProxyPort = 0
    Left = 448
    Top = 48
  end
end
