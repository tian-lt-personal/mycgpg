// pch
#include "pch.h"

// ShapeEditor headers
#include "DxRes.h"

namespace grph {

Context::Context(HWND hwnd, uint32_t width, uint32_t height) {
  {
    DXGI_SWAP_CHAIN_DESC swapchainDesc{
        .BufferDesc = {.Width = width,
                       .Height = height,
                       .RefreshRate = {.Numerator = 60, .Denominator = 1},
                       .Format = DXGI_FORMAT_R8G8B8A8_UNORM},
        .SampleDesc = {.Count = 1, .Quality = 0},
        .BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT,
        .BufferCount = 1,
        .OutputWindow = hwnd,
        .Windowed = true};
    D3D_FEATURE_LEVEL flvls[] = {D3D_FEATURE_LEVEL_11_1};
    check_hr(D3D11CreateDeviceAndSwapChain(
        nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr,
        D3D11_CREATE_DEVICE_SINGLETHREADED | D3D11_CREATE_DEVICE_BGRA_SUPPORT,
        flvls, 1, D3D11_SDK_VERSION, &swapchainDesc, schain_.put(),
        device_.put(), nullptr, devctx_.put()));
  }  // namespace grph
}

}  // namespace grph
