#pragma once

#include "pch.h"

namespace grph {

struct Vertices {
  wil::com_ptr<ID3D11Buffer> VertexBuffer;
};

class Pipeline {
 public:
  explicit Pipeline(ID3D11Device1* device);

 private:
  wil::com_ptr<ID3D11VertexShader> vs_;
  wil::com_ptr<ID3D11PixelShader> ps_;
};

class Context {
 public:
  explicit Context(HWND hwnd, uint32_t width, uint32_t height);
  void ResetDevice(uint32_t width, uint32_t height);
  void Resize(uint32_t width, uint32_t height);

  Pipeline* CreatePipeline();
  void Present() const;

 private:
  wil::com_ptr<ID3D11RenderTargetView> rtvScreen_;
  wil::com_ptr<IDXGISwapChain1> schain_;
  std::deque<Pipeline> pipelines_;
  wil::com_ptr<ID3D11DeviceContext1> devctx_;
  wil::com_ptr<ID3D11Device1> device_;
  uint32_t width_ = 0;
  uint32_t height_ = 0;
  HWND hwnd_ = nullptr;
};

}  // namespace grph
