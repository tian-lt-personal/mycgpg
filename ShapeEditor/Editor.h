#pragma once
#include "DxRes.h"

class Editor {
 public:
  struct Accessor;

 public:
  explicit Editor(HWND parent, uint32_t width, uint32_t height);

 private:
  HWND hwndParent_;
  std::optional<grph::Context> graphicsContext_;
};
