// pch
#include "pch.h"

// ShapeEditor headers
#include "DxRes.h"
#include "Shapes.h"

namespace {

class Rectangle : public Shape {
 public:
  virtual void Draw() const = 0;

 private:
  grph::Quad* vertices;
};

}  // namespace

std::unique_ptr<Shape> MakeShape(BuiltinShape) { return nullptr; }
