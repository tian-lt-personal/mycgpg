#pragma once

// std headers
#include <cstdint>
#include <map>

// dx headers
#include <DirectXMath.h>

struct Node {
  using IdType = uint32_t;

  DirectX::XMMATRIX Matrix = DirectX::XMMatrixIdentity();
  std::map<IdType, Node*> Children;
  IdType Id = 0;
};

struct Shape : Node {
  virtual ~Shape() = default;
  virtual void Draw() const = 0;
};

enum struct BuiltinShape { Rectangle, Circle };
std::unique_ptr<Shape> MakeShape(BuiltinShape type);
