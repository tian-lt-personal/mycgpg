// pch
#include "pch.h"

// ShapeEditor
#include "Editor.h"

struct Editor::Accessor {
  static void AssignGraphicsContext(Editor& self, grph::Context context) {
    self.graphicsContext_ = std::move(context);
  }
  static void Resize(Editor& self, uint32_t width, uint32_t height) {
    self.graphicsContext_->Resize(width, height);
  }
};

namespace {

using Accessor = Editor::Accessor;

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

Editor& Self(HWND hwnd) {
  return *reinterpret_cast<Editor*>(GetWindowLongPtrW(hwnd, GWLP_USERDATA));
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
  switch (msg) {
    case WM_SIZE:
      if (wparam == SIZE_RESTORED) {
        Accessor::Resize(Self(hwnd), LOWORD(lparam), HIWORD(lparam));
      }
      return 0;
    case WM_CREATE: {
      auto params = reinterpret_cast<CREATESTRUCTW*>(lparam);
      SetWindowLongPtrW(hwnd, GWLP_USERDATA,
                        reinterpret_cast<LONG_PTR>(params->lpCreateParams));
      Accessor::AssignGraphicsContext(
          Self(hwnd),
          grph::Context{params->hwndParent, static_cast<uint32_t>(params->cx),
                        static_cast<uint32_t>(params->cy)});
      return 0;
    }
  }
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
      0, width, height, parent, nullptr, hinst, this)};
  if (!hwnd.is_valid()) {
    throw std::runtime_error{"Failed to create Internal Editor Window."};
  }
}
