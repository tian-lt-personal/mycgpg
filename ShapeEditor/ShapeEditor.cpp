#include "pch.h"

extern "C" LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam,
                                    LPARAM lparam);
namespace {

void RegisterWndClass(HINSTANCE hinst) {
  WNDCLASSEXW wcex{.cbSize = sizeof(WNDCLASSEXW),
                   .style = CS_VREDRAW | CS_HREDRAW | CS_DROPSHADOW,
                   .lpfnWndProc = WndProc,
                   .hInstance = hinst,
                   .hCursor = LoadCursorW(NULL, IDC_ARROW),
                   .hbrBackground = (HBRUSH)(COLOR_WINDOW + 1),
                   .lpszClassName = L"ShapeEditorClass"};
  if (!RegisterClassExW(&wcex)) {
    throw std::runtime_error{"RegisterClassExW failed."};
  }
}

}  // namespace

extern "C" LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam,
                                    LPARAM lparam) {
  switch (msg) {
    case WM_DESTROY:
      PostQuitMessage(0);
      return 0;
  }
  return DefWindowProcW(hwnd, msg, wparam, lparam);
}

extern "C" int WINAPI wWinMain(HINSTANCE hinst, HINSTANCE, PWSTR, int cmdshow) {
  RegisterWndClass(hinst);
  wil::unique_hwnd hwnd{CreateWindowExW(
      0, L"ShapeEditorClass", L"Shape Editor", WS_OVERLAPPEDWINDOW,
      CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, nullptr,
      nullptr, hinst, nullptr)};
  if (!hwnd.is_valid()) {
    std::terminate();
  }
  ShowWindow(hwnd.get(), cmdshow);
  MSG msg;
  while (GetMessageW(&msg, nullptr, 0, 0)) {
    TranslateMessage(&msg);
    DispatchMessageW(&msg);
  }
  return 0;
}
