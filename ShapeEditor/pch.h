#pragma once
#ifndef UNICODE
#define UNICODE
#endif  // !UNICODE

// std headers
#include <cassert>
#include <memory>
#include <optional>
#include <stdexcept>

// windows headers
#include <Windows.h>

// wil headers
#include <wil/com.h>
#include <wil/common.h>
#include <wil/resource.h>

// dx headers
#include <d3d11.h>
#include <d3d11_1.h>
#pragma comment(lib, "D3D11.lib")

inline void check_hr(HRESULT hr) {
  if (!SUCCEEDED(hr)) {
    throw std::runtime_error{"bad hresult"};
  }
}
