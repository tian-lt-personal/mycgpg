// pch
#include "pch.h"

// ShapeEditor headers
#include "DxRes.h"

namespace grph {

std::string ReadBinaryFile(std::filesystem::path path) {
  std::ifstream file;
  file.exceptions(std::ios_base::failbit);
  file.open(path, std::ios_base::in | std::ios_base::binary);
  std::string content(std::istreambuf_iterator{file},
                      std::istreambuf_iterator<char>{});
  return content;
}

Context::Context(HWND hwnd, uint32_t width, uint32_t height) : hwnd_(hwnd) {
  ResetDevice(width, height);
}

void Context::ResetDevice(uint32_t width, uint32_t height) {
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

  for (auto& pl : pipelines_) {
    pl = Pipeline{device_.get()};
  }

  for (auto& q : quads_) {
    q = Quad{device_.get()};
  }

  Resize(width, height);
}

void Context::Resize(uint32_t width, uint32_t height) {
  if (width_ == width && height_ == height) {
    return;
  }
  if (schain_) {
    rtvScreen_.reset();
    devctx_->ClearState();
    auto hr =
        schain_->ResizeBuffers(2, width, height, DXGI_FORMAT_R8G8B8A8_UNORM, 0);
    if (hr == DXGI_ERROR_DEVICE_REMOVED || hr == DXGI_ERROR_DEVICE_RESET) {
      schain_.reset();
      ResetDevice(width, height);
    } else {
      check_hr(hr);
    }
  } else {
    DXGI_SWAP_CHAIN_DESC desc{
        .BufferDesc = {.Width = width,
                       .Height = height,
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
  width_ = width;
  height_ = height;
  wil::com_ptr<ID3D11Texture2D> backBuffer;
  check_hr(schain_->GetBuffer(0, IID_PPV_ARGS(backBuffer.put())));
  check_hr(device_->CreateRenderTargetView(backBuffer.get(), nullptr,
                                           rtvScreen_.put()));
}

void Context::Present() const {
  constexpr float clr[] = {0.2f, 0.2f, 0.2f, 1.f};
  devctx_->ClearRenderTargetView(rtvScreen_.get(), clr);
  schain_->Present(1, 0);
  devctx_->DiscardView1(rtvScreen_.get(), nullptr, 0);
}

Pipeline* Context::CreatePipeline() {
  pipelines_.push_back(Pipeline{device_.get()});
  return &pipelines_.back();
}

Quad* Context::CreateQuad() {
  quads_.push_back(Quad{device_.get()});
  return &quads_.back();
}

Pipeline::Pipeline(ID3D11Device1* device) {
  auto code = ReadBinaryFile("PsSimple.cso");
  check_hr(device->CreatePixelShader(code.data(), code.length(), nullptr,
                                     ps_.put()));
  code = ReadBinaryFile("VsSimple2d.cso");
  check_hr(device->CreateVertexShader(code.data(), code.length(), nullptr,
                                      vs_.put()));
  {
    D3D11_INPUT_ELEMENT_DESC elements[] = {
        {.SemanticName = "POS",
         .Format = DXGI_FORMAT_R32G32_FLOAT,
         .InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA},
        {.SemanticName = "COL",
         .Format = DXGI_FORMAT_R32G32B32A32_FLOAT,
         .AlignedByteOffset = D3D11_APPEND_ALIGNED_ELEMENT,
         .InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA}};
    check_hr(device->CreateInputLayout(
        elements, static_cast<UINT>(std::ranges::size(elements)), code.data(),
        code.length(), vlayout_.put()));
  }
}

Quad::Quad(ID3D11Device1* device) {
  constexpr DirectX::XMFLOAT4A Green{0.1f, 1.f, 0.1f, 1.f};
  Vertex vertices[] = {// the left half
                       {.Pos = {-1.f, 1.f}, .Col = Green},
                       {.Pos = {1.f, -1.f}, .Col = Green},
                       {.Pos = {-1.f, -1.f}, .Col = Green},
                       // the right half
                       {.Pos = {1.f, -1.f}, .Col = Green},
                       {.Pos = {-1.f, 1.f}, .Col = Green},
                       {.Pos = {1.f, 1.f}, .Col = Green}};
  D3D11_BUFFER_DESC desc{.ByteWidth = sizeof(vertices),
                         .Usage = D3D11_USAGE_IMMUTABLE,
                         .BindFlags = D3D11_BIND_VERTEX_BUFFER};
  D3D11_SUBRESOURCE_DATA data{.pSysMem = vertices};
  check_hr(device->CreateBuffer(&desc, &data, v_.put()));
}

}  // namespace grph
