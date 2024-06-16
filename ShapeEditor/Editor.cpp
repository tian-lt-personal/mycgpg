// pch
#include "pch.h"

// ShapeEditor
#include "Editor.h"

namespace {

ATOM RegisterEditorClass(HINSTANCE hinst, WNDPROC wndproc) {
  WNDCLASSEXW wcex{.cbSize = sizeof(WNDCLASSEXW),
                   .style = CS_VREDRAW | CS_HREDRAW,
                   .lpfnWndProc = wndproc,
                   .hInstance = hinst,
                   .hCursor = LoadCursorW(NULL, IDC_ARROW),
                   .hbrBackground = (HBRUSH)(COLOR_WINDOW + 1),
                   .lpszClassName = L"InternalEditorClass"};
  auto result = RegisterClassExW(&wcex);
  if (!result) {
    throw std::runtime_error{"RegisterClassExW failed."};
  }
  return result;
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
  return DefWindowProcW(hwnd, msg, wparam, lparam);
}

}  // namespace

Editor::Editor(HWND parent, uint32_t width, uint32_t height)
    : hwndParent_(parent) {
  auto hinst =
      reinterpret_cast<HINSTANCE>(GetWindowLongPtrW(parent, GWLP_HINSTANCE));
  static auto registerResult = RegisterEditorClass(hinst, WndProc);
  wil::unique_hwnd hwnd{CreateWindowExW(
      0, L"InternalEditorClass", L"Internal Editor", WS_CHILD | WS_VISIBLE, 0,
      0, width, height, parent, nullptr, hinst, nullptr)};
  if (!hwnd.is_valid()) {
    throw std::runtime_error{"Failed to create Internal Editor Window."};
  }
  graphicsContext_ = grph::Context{parent, width, height};
}
