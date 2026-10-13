import { useEffect, useState, type FormEvent } from 'react';
import { api, type DocumentSummary, type RetrieveResult } from '../api';

export function RetrievalLesson() {
  const [documents, setDocuments] = useState<DocumentSummary[]>([]);
  const [query, setQuery] = useState('How does overlap help chunking?');
  const [topK, setTopK] = useState(5);
  const [useVector, setUseVector] = useState(true);
  const [useBm25, setUseBm25] = useState(false);
  const [fileName, setFileName] = useState('');
  const [result, setResult] = useState<RetrieveResult | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.documents().then(setDocuments).catch((e: Error) => setError(e.message));
  }, []);

  async function search(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError(null);
    try {
      setResult(await api.retrieve({ query, topK, useVector, useBm25, fileName: fileName || undefined }));
    } catch (e) {
      setError((e as Error).message);
      setResult(null);
    } finally {
      setBusy(false);
    }
  }

  return (
    <section>
      {error && <p className="error">{error}</p>}

      <form className="card" onSubmit={search}>
        <label className="block">
          Query
          <input value={query} onChange={(e) => setQuery(e.target.value)} />
        </label>
        <div className="row">
          <label>
            Top K{' '}
            <input type="number" min={1} max={20} value={topK} onChange={(e) => setTopK(Number(e.target.value))} />
          </label>
          <label>
            <input type="checkbox" checked={useVector} onChange={(e) => setUseVector(e.target.checked)} /> Vector
          </label>
          <label>
            <input type="checkbox" checked={useBm25} onChange={(e) => setUseBm25(e.target.checked)} /> BM25
          </label>
          <label>
            Document{' '}
            <select value={fileName} onChange={(e) => setFileName(e.target.value)}>
              <option value="">All documents</option>
              {documents.map((d) => (
                <option key={d.id} value={d.fileName}>{d.fileName}</option>
              ))}
            </select>
          </label>
        </div>
        <button type="submit" disabled={busy || !query.trim() || (!useVector && !useBm25)}>
          {busy ? 'Searching...' : 'Search'}
        </button>
      </form>

      {result && (
        <>
          <p className="muted">
            {result.candidateCount} candidate chunk(s) after filters, {result.embeddedCandidateCount} with embeddings.
          </p>

          {result.chunks.length === 0 && <p className="muted">No matching chunks.</p>}

          {result.chunks.map((c) => (
            <div className="card chunk" key={c.chunkId}>
              <div className="card-header">
                <strong>#{c.rank}</strong>
                <span className="muted">{c.fileName} · {c.heading ?? c.title}</span>
                <span className="badge">score {c.score.toFixed(4)}</span>
              </div>
              <p className="muted small">
                {c.vectorScore !== null && <>vector #{c.vectorRank} · cosine {c.vectorScore.toFixed(3)} </>}
                {c.bm25Rank !== null && <>· bm25 #{c.bm25Rank}</>}
              </p>
              <pre>{c.content}</pre>
            </div>
          ))}

          <div className="card">
            <h3>Pipeline</h3>
            <ol>
              {result.stages.map((stage) => (
                <li key={stage}>{stage}</li>
              ))}
            </ol>
          </div>
        </>
      )}
    </section>
  );
}
