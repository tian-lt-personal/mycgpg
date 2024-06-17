// pch
#include "pch.h"

// ShapeEditor
#include "Editor.h"

struct Editor::Accessor {
  static void AssignGraphicsContext(Editor& self, grph::Context context) {
    self.graphicsContext_ = std::move(context);
  }
  static void Repaint(Editor& self) { self.graphicsContext_->Present(); }
};

namespace {

using Accessor = Editor::Accessor;

ATOM RegisterEditorClass(HINSTANCE hinst, WNDPROC wndproc) {
  WNDCLASSEXW wcex{.cbSize = sizeof(WNDCLASSEXW),
                   .style = CS_VREDRAW | CS_HREDRAW,
                   .lpfnWndProc = wndproc,
                   .hInstance = hinst,
                   .hCursor = LoadCursorW(NULL, IDC_ARROW),
                   .hbrBackground = (HBRUSH)(COLOR_WINDOW),
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
    case WM_PAINT: {
      PAINTSTRUCT ps;
      BeginPaint(hwnd, &ps);
      Accessor::Repaint(Self(hwnd));
      EndPaint(hwnd, &ps);
      return 0;
    }
    case WM_CREATE: {
      auto params = reinterpret_cast<CREATESTRUCTW*>(lparam);
      SetWindowLongPtrW(hwnd, GWLP_USERDATA,
                        reinterpret_cast<LONG_PTR>(params->lpCreateParams));
      Accessor::AssignGraphicsContext(
          Self(hwnd), grph::Context{hwnd, static_cast<uint32_t>(params->cx),
                                    static_cast<uint32_t>(params->cy)});
      return 0;
    }
  }
  return DefWindowProcW(hwnd, msg, wparam, lparam);
}

}  // namespace

Editor::Editor(HWND parent, int x, int y, uint32_t width, uint32_t height)
    : hwndParent_(parent) {
  auto hinst =
      reinterpret_cast<HINSTANCE>(GetWindowLongPtrW(parent, GWLP_HINSTANCE));
  static auto registerResult = RegisterEditorClass(hinst, WndProc);
  hwnd_ = wil::unique_hwnd{
      CreateWindowExW(0, L"InternalEditorClass", L"Internal Editor",
                      WS_CHILD | WS_VISIBLE | WS_BORDER, x, y, width, height,
                      parent, nullptr, hinst, this)};
  if (!hwnd_.is_valid()) {
    throw std::runtime_error{"Failed to create Internal Editor Window."};
  }
  assert(graphicsContext_.has_value());
}

Editor::Editor(Editor&& rhs) noexcept
    : graphicsContext_(std::move(rhs.graphicsContext_)),
      hwnd_(std::move(rhs.hwnd_)),
      hwndParent_(std::exchange(rhs.hwndParent_, nullptr)) {
  if (hwnd_.is_valid()) {
    SetWindowLongPtrW(hwnd_.get(), GWLP_USERDATA,
                      reinterpret_cast<LONG_PTR>(this));
  }
}
Editor& Editor::operator=(Editor&& rhs) noexcept {
  graphicsContext_ = std::move(rhs.graphicsContext_);
  hwnd_ = std::move(rhs.hwnd_);
  hwndParent_ = std::exchange(rhs.hwndParent_, nullptr);
  if (hwnd_.is_valid()) {
    SetWindowLongPtrW(hwnd_.get(), GWLP_USERDATA,
                      reinterpret_cast<LONG_PTR>(this));
  }
  return *this;
}

void Editor::Resize(uint32_t width, uint32_t height) {
  SetWindowPos(hwnd_.get(), nullptr, 0, 0, width, height,
               SWP_NOMOVE | SWP_NOZORDER);
  graphicsContext_->Resize(width, height);
}
