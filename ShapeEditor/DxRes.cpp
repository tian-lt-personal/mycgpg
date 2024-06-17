// pch
#include "pch.h"

// ShapeEditor headers
#include "DxRes.h"

namespace grph {

Context::Context(HWND hwnd, uint32_t width, uint32_t height)
    : width_(width), height_(height), hwnd_(hwnd) {
  ResetDevice();
}

void Context::ResetDevice() {
  D3D_FEATURE_LEVEL flvls[] = {D3D_FEATURE_LEVEL_11_1};
  check_hr(D3D11CreateDevice(
      nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr,
      D3D11_CREATE_DEVICE_SINGLETHREADED | D3D11_CREATE_DEVICE_BGRA_SUPPORT,
      flvls, 1, D3D11_SDK_VERSION, device_.put(), nullptr, devctx_.put()));
  Resize(width_, height_);
}

void Context::Resize(uint32_t width, uint32_t height) {
  width_ = width;
  height_ = height;
  if (schain_) {
    check_hr(schain_->ResizeBuffers(2, width_, height_,
                                    DXGI_FORMAT_R8G8B8A8_UNORM, 0));
  } else {
    DXGI_SWAP_CHAIN_DESC desc{
        .BufferDesc = {.Width = width_,
                       .Height = height_,
                       .RefreshRate = {.Numerator = 60, .Denominator = 1},
                       .Format = DXGI_FORMAT_R8G8B8A8_UNORM},
        .SampleDesc = {.Count = 1, .Quality = 0},
        .BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT,
        .BufferCount = 2,
        .OutputWindow = hwnd_,
        .Windowed = true};
    auto dxgiDevice = device_.query<IDXGIDevice>();
    wil::com_ptr<IDXGIAdapter> dxgiAdapter;
    check_hr(dxgiDevice->GetAdapter(dxgiAdapter.put()));
    wil::com_ptr<IDXGIFactory> dxgiFactory;
    check_hr(dxgiAdapter->GetParent(IID_PPV_ARGS(dxgiFactory.put())));
    check_hr(dxgiFactory->CreateSwapChain(device_.get(), &desc, schain_.put()));
  }
}

}  // namespace grph
