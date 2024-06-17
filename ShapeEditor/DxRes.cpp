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
  UINT flags =
      D3D11_CREATE_DEVICE_SINGLETHREADED | D3D11_CREATE_DEVICE_BGRA_SUPPORT;
#ifdef _DEBUG
  flags |= D3D11_CREATE_DEVICE_DEBUG;
#endif  // _DEBUG

  wil::com_ptr<ID3D11Device> device;
  check_hr(D3D11CreateDevice(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, flags,
                             flvls, 1, D3D11_SDK_VERSION, device.put(), nullptr,
                             nullptr));
  device_ = device.query<ID3D11Device1>();
  device_->GetImmediateContext1(devctx_.put());
  Resize(width_, height_);
}

void Context::Resize(uint32_t width, uint32_t height) {
  width_ = width;
  height_ = height;
  if (schain_) {
    rtvScreen_.reset();
    devctx_->ClearState();
    auto hr = schain_->ResizeBuffers(2, width_, height_,
                                     DXGI_FORMAT_R8G8B8A8_UNORM, 0);
    if (hr == DXGI_ERROR_DEVICE_REMOVED || hr == DXGI_ERROR_DEVICE_RESET) {
      schain_.reset();
      ResetDevice();
    } else {
      check_hr(hr);
    }
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
        .Windowed = true,
        .SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD};
    auto dxgiDevice = device_.query<IDXGIDevice>();
    wil::com_ptr<IDXGIAdapter> dxgiAdapter;
    check_hr(dxgiDevice->GetAdapter(dxgiAdapter.put()));
    wil::com_ptr<IDXGIFactory> dxgiFactory;
    check_hr(dxgiAdapter->GetParent(IID_PPV_ARGS(dxgiFactory.put())));
    wil::com_ptr<IDXGISwapChain> schain;
    check_hr(dxgiFactory->CreateSwapChain(device_.get(), &desc, schain.put()));
    schain_ = schain.query<IDXGISwapChain1>();
  }
  wil::com_ptr<ID3D11Texture2D> backBuffer;
  check_hr(schain_->GetBuffer(0, IID_PPV_ARGS(backBuffer.put())));
  check_hr(device_->CreateRenderTargetView(backBuffer.get(), nullptr,
                                           rtvScreen_.put()));
}

void Context::Present() const {
  schain_->Present(1, 0);
  devctx_->DiscardView1(rtvScreen_.get(), nullptr, 0);
}

}  // namespace grph
