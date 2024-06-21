#pragma once
#ifndef UNICODE
#define UNICODE
#endif  // !UNICODE

// std headers
#include <cassert>
#include <deque>
#include <filesystem>
#include <fstream>
#include <iterator>
#include <memory>
#include <optional>
#include <ranges>
#include <set>
#include <stdexcept>
#include <vector>

// windows headers
#include <Windows.h>
#include <windowsx.h>

// wil headers
#include <wil/com.h>
#include <wil/common.h>
#include <wil/resource.h>

// dx headers
#include <DirectXMath.h>
#include <d3d11.h>
#include <d3d11_1.h>
#pragma comment(lib, "D3D11.lib")

#pragma warning(disable : 4324)

inline void check_hr(HRESULT hr) {
  if (!SUCCEEDED(hr)) {
    throw std::runtime_error{"bad hresult"};
  }
}
