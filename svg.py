#!/usr/bin/env python
import pygal
import csv
import sys
import string
from collections import namedtuple
from pygal.style import CleanStyle


implementations = ['DevIL', 'Pfim', 'TargaImage']
Benchmark = namedtuple('Benchmark', ['title'] + implementations)


def create_benchmarks(path):
    with open(path, 'rb') as csvfile:
        rows = list(csv.reader(csvfile))
        header = rows[0]
        benches = map(Benchmark._make, rows[1:])
        for bench in benches:
            bar_chart = pygal.HorizontalBar(style=CleanStyle)
            bar_chart.title = 'Relative time to load a ' + bench.title
            for index, x in enumerate(bench[1:]):
                bar_chart.add(implementations[index], float(x))
            friendly_title = bench.title.translate(string.maketrans(' ', '-'))
            bar_chart.render_to_file('docs/output/img/{}.svg'.format(friendly_title))


if __name__ == '__main__':
    create_benchmarks(sys.argv[1])