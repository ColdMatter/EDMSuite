import ctypes

try:
    a = ctypes.cdll.LoadLibrary(r'C:\EDMSuite\CaFCon\bin\x64\Debug\CaFCon.dll')
    print('lol')
    print(a.iexist())

    input("Press Enter to continue...")
except Exception as e:
    print(e)
    input("Press Enter to continue...")
    raise e from e