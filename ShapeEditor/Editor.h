#pragma once
#include "DxRes.h"

class Editor {
 public:
  struct Accessor;

 public:
  explicit Editor(HWND parent, int x, int y, uint32_t width, uint32_t height);
  void Resize(uint32_t width, uint32_t height);
  void Tick() const;

 private:
  HWND hwndParent_;
  std::optional<grph::Context> graphicsContext_;
};
