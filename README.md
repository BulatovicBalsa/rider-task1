# Problems

## Equality Semantics

- (1, 2) == (1, 2)  # True
- new Point(1, 2) == new Point(1, 2)  # False

## Deconstruction Syntax

- var (x, y) = (1, 2);
- var (x, y) = new Point(1, 2);  # Error

## Immutability

- var point = new Point(1, 2);
- point.X = 3;  # Error

## Memory Allocation

- var point = new Point(1, 2);  # Allocates on the heap
- var tuple = (1, 2);  # Allocates on the stack

## Shape-only Matching

- Coordinate (x, y) = new Point(1, 2);  # Wanted behavior
- Range (start, end) = new Point(1, 2);  # Unwanted behavior