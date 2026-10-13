# Chunking

Chunking is the process of splitting a long document into smaller pieces before embedding it. Embedding models have input limits, and a single vector for a 50-page document would blur many topics together.

## Chunk size

Chunk size controls how much text each piece contains. Small chunks are precise but may lose context. Large chunks keep context but may mix several topics, which makes the vector less focused. A common starting point is 500 to 1000 characters.

## Overlap

Overlap repeats the end of one chunk at the start of the next. If a sentence that answers a question falls exactly on a boundary, overlap ensures it still appears completely in at least one chunk.

## Structure-aware splitting

Markdown documents already have structure. Splitting on headings keeps each section together, and splitting long sections on blank lines keeps paragraphs intact. When a paragraph is still too long, the splitter cuts it by size as a last resort.

## Trade-offs

There is no single best setting. Try different values and measure retrieval quality with a set of questions where you know which chunk should be found.
