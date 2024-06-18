#pragma once
#include "DxRes.h"
#include "Shapes.h"

class Editor {
 public:
  struct Accessor;

 public:
  explicit Editor(HWND parent, int x, int y, uint32_t width, uint32_t height);
  Editor(const Editor&) = delete;
  Editor(Editor&& rhs) noexcept;
  Editor& operator=(const Editor&) = delete;
  Editor& operator=(Editor&& rhs) noexcept;

  void Resize(uint32_t width, uint32_t height);

 private:
  Node root;
  std::optional<grph::Context> graphicsContext_;
  wil::unique_hwnd hwnd_;
  HWND hwndParent_;
};
