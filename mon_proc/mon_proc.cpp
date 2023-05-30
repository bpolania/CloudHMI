//---------------------------------------------------------------------------

#include <vcl.h>
#pragma hdrstop
#include <dir.h>
USERES("mon_proc.res");
USEFORM("proc.cpp", FormMain);
//---------------------------------------------------------------------------
WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int)
{
  try
  {
     ::CreateMutex( NULL, FALSE, "Mon_ProcOSHMI" );
     if ( GetLastError() != ERROR_ALREADY_EXISTS )
       { // outra instancia j� est� rodando
       // vai para o diret�rio do execut�vel
       chdir( ExtractFilePath(Application->ExeName).c_str() );

       Application->Initialize();
       Application->CreateForm(__classid(TFormMain), &FormMain);
       Application->Run();
       }
  }
  catch (Exception &exception)
  {
     Application->ShowException(&exception);
  }
  return 0;
}
//---------------------------------------------------------------------------
