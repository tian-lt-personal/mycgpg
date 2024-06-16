#pragma once

#include "pch.h"

namespace grph {

class Context {
 public:
  explicit Context(HWND hwnd, uint32_t width, uint32_t height);

 private:
  wil::com_ptr<IDXGISwapChain> schain_;
  wil::com_ptr<ID3D11DeviceContext> devctx_;
  wil::com_ptr<ID3D11Device> device_;
};

struct Pipeline {};

}  // namespace grph
