// pch
#include "pch.h"

// ShapeEditor
#include "Editor.h"

namespace {

void RegisterWndClass(HINSTANCE hinst, WNDPROC wndproc) {
  WNDCLASSEXW wcex{.cbSize = sizeof(WNDCLASSEXW),
                   .style = CS_VREDRAW | CS_HREDRAW | CS_DROPSHADOW,
                   .lpfnWndProc = wndproc,
                   .hInstance = hinst,
                   .hCursor = LoadCursorW(NULL, IDC_ARROW),
                   .hbrBackground = (HBRUSH)(COLOR_WINDOW + 1),
                   .lpszClassName = L"ShapeEditorClass"};
  if (!RegisterClassExW(&wcex)) {
    throw std::runtime_error{"RegisterClassExW failed."};
  }
}

struct WindowContext {
  Editor Editor;
};

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
  switch (msg) {
    case WM_CREATE: {
      auto params = reinterpret_cast<CREATESTRUCTW*>(lparam);
      auto ctx = reinterpret_cast<std::optional<WindowContext>*>(
          params->lpCreateParams);
      Editor editor{hwnd, static_cast<uint32_t>(params->cx),
                    static_cast<uint32_t>(params->cy)};
      *ctx = WindowContext{.Editor = std::move(editor)};
      SetWindowLongPtrW(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(&*ctx));
      return 0;
    }
    case WM_DESTROY:
      PostQuitMessage(0);
      return 0;
  }
  return DefWindowProcW(hwnd, msg, wparam, lparam);
}

}  // namespace

extern "C" int WINAPI wWinMain(HINSTANCE hinst, HINSTANCE, PWSTR, int cmdshow) {
  RegisterWndClass(hinst, WndProc);
  std::optional<WindowContext> ctx;  // use optional for delay-init
  wil::unique_hwnd hwnd{CreateWindowExW(
      0, L"ShapeEditorClass", L"Shape Editor", WS_OVERLAPPEDWINDOW,
      CW_USEDEFAULT, CW_USEDEFAULT, 1200, 900, nullptr, nullptr, hinst, &ctx)};
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
