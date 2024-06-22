#pragma once
#include "DxRes.h"
#include "Shapes.h"

namespace {

class Creator {
 public:
  explicit Creator(grph::Context* gctx);

 private:
  grph::Context* gctx_;
};

}  // namespace

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
  Node root_;
  std::optional<grph::Context> graphicsContext_;
  grph::Pipeline* pipeline_;
  grph::Quad* quad_;
  wil::unique_hwnd hwnd_;
  std::optional<Creator> creator_;
  HWND hwndParent_;
};
