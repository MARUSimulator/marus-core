// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// Constants for shader graph. For example, we can force shader features when we have yet to make a keyword for it in
// shader graph.

// This file must be included before all other includes. And it must be done for every node. This is due to #ifndef
// limiting includes from being evaluated once, and we cannot specify the order because shader graph does this.

#ifndef CREST_SHADERGRAPH_CONSTANTS_H
#define CREST_SHADERGRAPH_CONSTANTS_H

#define _SUBSURFACESCATTERING_ON 1

#endif // CREST_SHADERGRAPH_CONSTANTS_H
