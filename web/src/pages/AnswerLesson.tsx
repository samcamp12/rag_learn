import { useState, type FormEvent } from 'react';
import { api, type AskResult } from '../api';

export function AnswerLesson() {
  const [question, setQuestion] = useState('What is chunk overlap and why is it used?');
  const [topK, setTopK] = useState(4);
  const [result, setResult] = useState<AskResult | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function ask(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError(null);
    try {
      setResult(await api.ask({ query: question, topK }));
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

      <form className="card" onSubmit={ask}>
        <label className="block">
          Question
          <input value={question} onChange={(e) => setQuestion(e.target.value)} />
        </label>
        <label>
          Top K{' '}
          <input type="number" min={1} max={20} value={topK} onChange={(e) => setTopK(Number(e.target.value))} />
        </label>
        <div>
          <button type="submit" disabled={busy || !question.trim()}>
            {busy ? 'Thinking...' : 'Ask'}
          </button>
        </div>
      </form>

      {result && (
        <>
          <div className="card">
            <h3>Answer</h3>
            <p style={{ whiteSpace: 'pre-wrap' }}>{result.answer}</p>
          </div>

          <h3>Sources</h3>
          {result.sources.map((s, i) => (
            <div className="card chunk" key={s.chunkId}>
              <div className="card-header">
                <strong>[{i + 1}]</strong>
                <span className="muted">{s.fileName} · {s.heading ?? s.title}</span>
              </div>
              <pre>{s.content}</pre>
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
