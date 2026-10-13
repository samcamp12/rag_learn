export type Status = {
  databasePath: string;
  docsPath: string;
  documentCount: number;
  chunkCount: number;
  embeddedChunkCount: number;
  openAiConfigured: boolean;
};

export type DocumentSummary = {
  id: number;
  fileName: string;
  title: string;
  contentLength: number;
  chunkCount: number;
  ingestedAtUtc: string;
};

export type ChunkDto = {
  id: number;
  position: number;
  heading: string | null;
  charCount: number;
  content: string;
};

export type IngestResult = {
  documentsProcessed: number;
  chunksCreated: number;
};

export type SqlResult = {
  columns: string[];
  rows: unknown[][];
};

export type RetrieveOptions = {
  query: string;
  topK?: number;
  useVector?: boolean;
  useBm25?: boolean;
  fileName?: string;
  sectionContains?: string;
};

export type RetrievedChunk = {
  chunkId: number;
  documentId: number;
  fileName: string;
  title: string;
  heading: string | null;
  content: string;
  rank: number;
  score: number;
  vectorRank: number | null;
  vectorScore: number | null;
  bm25Rank: number | null;
};

export type RetrieveResult = {
  chunks: RetrievedChunk[];
  candidateCount: number;
  embeddedCandidateCount: number;
  stages: string[];
};

export type AskResult = {
  answer: string;
  sources: RetrievedChunk[];
  stages: string[];
};

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  });
  const body = await response.json().catch(() => null);
  if (!response.ok) {
    throw new Error(body?.error ?? `Request failed with status ${response.status}`);
  }
  return body as T;
}

export const api = {
  health: () => request<{ status: string; utc: string }>('/api/health'),
  status: () => request<Status>('/api/status'),
  documents: () => request<DocumentSummary[]>('/api/documents'),
  chunks: (documentId: number) => request<ChunkDto[]>(`/api/documents/${documentId}/chunks`),
  ingest: () => request<IngestResult>('/api/ingest', { method: 'POST' }),
  runSql: (sql: string) =>
    request<SqlResult>('/api/sql', { method: 'POST', body: JSON.stringify({ sql }) }),
  embed: () => request<{ embedded: number }>('/api/embed', { method: 'POST' }),
  retrieve: (options: RetrieveOptions) =>
    request<RetrieveResult>('/api/retrieve', { method: 'POST', body: JSON.stringify(options) }),
  ask: (options: RetrieveOptions) =>
    request<AskResult>('/api/ask', { method: 'POST', body: JSON.stringify(options) }),
};
